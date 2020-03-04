using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using Newtonsoft.Json;
using ReactiveUI;
using SocketIOClient;
using SatPlaceClient.Models.Json;
using SatPlaceClient.Models;
using System.Threading.Tasks;
using System.Reactive.Linq;
using SocketIOClient.Arguments;
using System.Numerics;
using SatPlaceClient.Extensions;
using SatPlaceClient.ImageProcessing;
using SatPlaceClient.ImageProcessing.Codecs.Png;

namespace SatPlaceClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            SatPlaceClient = new SocketIO("https://api.satoshis.place");

            DoTryConnecting();

            SetupHandlers();

            var CanRefreshCanvas = this.WhenAnyValue(x => x.CanvasRefreshInProgress, x => x.ConnectionReady)
                                       .Select(x => !x.Item1 & x.Item2)
                                       .ObserveOn(RxApp.MainThreadScheduler);

            RefreshCanvasCommand = ReactiveCommand.CreateFromTask(DoRefreshCanvasAsync, CanRefreshCanvas);

            ReconnectCommand = ReactiveCommand.Create(DoReconnect);

            this.WhenAnyValue(x => x.TargetImageFile)
                .Where(x => x != null)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(ImageFileOpened);
        }

        private SocketIO SatPlaceClient { get; }
        private GenericBitmap _latestCanvasBitmap;
        private OrderSettingsResult _orderSettings;

        private bool _connectionReady, _canvasRefreshInProgress, _enableReconnection, _pngFileProcessingInProgress;
        private byte _retryCounter;
        private string _targetImageFile, _errorMessage;
        public uint MaximumReconnectionAttempt { get; } = 6;

        /// <summary>
        /// Stores the latest bitmap data of satoshis.place's canvas.
        /// </summary>
        public GenericBitmap LatestCanvasBitmap
        {
            get => _latestCanvasBitmap;
            private set => this.RaiseAndSetIfChanged(ref _latestCanvasBitmap, value, nameof(LatestCanvasBitmap));
        }

        /// <summary>
        /// Signals if the connection to the backend is ready.
        /// </summary>
        public bool ConnectionReady
        {
            get => _connectionReady;
            private set => this.RaiseAndSetIfChanged(ref _connectionReady, value, nameof(ConnectionReady));
        }

        /// <summary>
        /// Signals if the canvas is currently refreshing.
        /// </summary>
        public bool CanvasRefreshInProgress
        {
            get => _canvasRefreshInProgress;
            private set => this.RaiseAndSetIfChanged(ref _canvasRefreshInProgress, value, nameof(CanvasRefreshInProgress));
        }

        public bool EnableReconnection
        {
            get => _enableReconnection;
            private set => this.RaiseAndSetIfChanged(ref _enableReconnection, value, nameof(EnableReconnection));
        }

        public bool PNGFileProcessingInProgress
        {
            get => _pngFileProcessingInProgress;
            private set => this.RaiseAndSetIfChanged(ref _pngFileProcessingInProgress, value, nameof(PNGFileProcessingInProgress));
        }

        public string TargetImageFile
        {
            get => _targetImageFile;
            set => this.RaiseAndSetIfChanged(ref _targetImageFile, value, nameof(TargetImageFile));
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value, nameof(ErrorMessage));
        }

        public OrderSettingsResult OrderSettings
        {
            get => _orderSettings;
            private set => this.RaiseAndSetIfChanged(ref _orderSettings, value, nameof(OrderSettings));
        }

        public ReactiveCommand<Unit, Unit> RefreshCanvasCommand { get; }
        public ReactiveCommand<Unit, Unit> ReconnectCommand { get; }

        private void ImageFileOpened(string path)
        {
            try
            {
                PNGFileProcessingInProgress = true;
                DoImageProcessing(path);
            }
            catch (Exception e)
            {
                DisplayError(e.Message);
            }
            finally
            {
                PNGFileProcessingInProgress = false;
            }
        }

        private void DisplayError(string errorMsg)
        {
            ErrorMessage = errorMsg;
#pragma warning disable
            TimeOutErrorMessage();
#pragma warning restore
        }

        private async Task TimeOutErrorMessage()
        {
            await Task.Delay(TimeSpan.FromSeconds(8));
            ErrorMessage = null;
        }

        private void DoImageProcessing(string path)
        {
            var target = Png.Open(path);
            var tW = target.Width;
            var tH = target.Height;
            var totalPixelCount = tW * tH;
            var limitAxis = Math.Round(Math.Sqrt(OrderSettings.OrderPixelsLimit));

            if (totalPixelCount > OrderSettings.OrderPixelsLimit)
                throw new Exception($"Picture size exceeds the allowed dimensions. Make sure that the image only contains {OrderSettings.OrderPixelsLimit:###,###,###} pixels or is under {limitAxis}x{limitAxis} pixels.");

            var background = GenericColor.FromVector4(Vector4.One);

            var newTargetImage = new GenericBitmap(tW, tH, new GenericColor[totalPixelCount]);
            var ditheringArray = new Vector3[totalPixelCount];
            var allowedPaletteLab = new List<Rgb>(OrderSettings.Colors.Length);

            foreach (var color in OrderSettings.Colors)
            {
                var lab = new Rgb(color.R, color.G, color.B);
                allowedPaletteLab.Add(lab);
            }

            var nearestColorCmp = new ImageProcessing.Comparisons.CieDe2000Comparison();

            int CoordsToIndex(int x, int y) => x + tW * y;

            for (int y = 0; y < tH; y++)
                for (int x = 0; x < tW; x++)
                {
                    var pngPixel = target.GetPixel(x, y);
                    var orig = new GenericColor(pngPixel.R, pngPixel.G, pngPixel.B, byte.MaxValue);
                    var blendedPixelRgb = background.AlphaBlend(orig);

                    ditheringArray[CoordsToIndex(x, y)] = blendedPixelRgb.ToVector3();
                }

            for (int y = 0; y < tH; y++)
                for (int x = 0; x < tW; x++)
                {
                    var oldpixel = ditheringArray[CoordsToIndex(x, y)];

                    // Use CIEDE2000 color perception delta to get the closest color match
                    var newpixel = new GenericColor(allowedPaletteLab.Select(match =>
                                                          (match,
                                                           nearestColorCmp.Compare(new GenericColor(oldpixel).ToRgb(),
                                                           match)))
                                                          .MinBy(x => x.Item2)
                                                          .match).ToVector3();


                    newTargetImage.Pixels[CoordsToIndex(x, y)] = new GenericColor(newpixel);

                    var quant_error = Vector3.Clamp(newpixel - oldpixel, Vector3.Zero, Vector3.One);

                    // if (quant_error.Length() < 0 | quant_error.Length() > 0)
                    // {

                    // }

                    var coord = CoordsToIndex(x + 1, y);

                    if (coord >= 0 && coord < totalPixelCount)
                    {
                        ditheringArray[coord] += quant_error * (7f / 8f);
                    }

                    coord = CoordsToIndex(x - 1, y);

                    if (coord >= 0 && coord < totalPixelCount)
                    {
                        ditheringArray[coord] += quant_error * (3f / 8f);
                    }

                    coord = CoordsToIndex(x, y + 1);

                    if (coord >= 0 && coord < totalPixelCount)
                    {
                        ditheringArray[coord] += quant_error * (5f / 8f);
                    }

                    coord = CoordsToIndex(x + 1, y + 1);

                    if (coord >= 0 && coord < totalPixelCount)
                    {
                        ditheringArray[coord] += quant_error * (1f / 8f);
                    }
                }

            LatestCanvasBitmap = newTargetImage;
        }


        private async void DoTryConnecting()
        {
            try
            {
                await SatPlaceClient.ConnectAsync();
                _retryCounter = 0;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, _retryCounter)));
                _retryCounter++;

                if (_retryCounter <= MaximumReconnectionAttempt)
                {
                    DoTryConnecting();
                }
                else
                {
                    EnableReconnection = true;
                }
            }
        }

        private async Task DoRefreshCanvasAsync()
        {
            if (ConnectionReady & !CanvasRefreshInProgress)
            {
                CanvasRefreshInProgress = true;
                await SatPlaceClient.EmitAsync("GET_LATEST_PIXELS", null);
                await SatPlaceClient.EmitAsync("GET_SETTINGS", null);
            }
        }

        private async Task GetSettingsAsync()
        {
            if (ConnectionReady)
            {
                CanvasRefreshInProgress = true;
                await SatPlaceClient.EmitAsync("GET_SETTINGS_RESULT", null);
            }
        }

        private void SetupHandlers()
        {
            SatPlaceClient.On("GET_LATEST_PIXELS_RESULT", HandleLatestCanvasData);
            SatPlaceClient.On("GET_SETTINGS_RESULT", HandleGetSettingsResult);
            SatPlaceClient.On("BROADCAST_STATS", HandleBroadcastHeartbeat);

            SatPlaceClient.OnConnected += async delegate
            {
                ConnectionReady = true;
                await DoRefreshCanvasAsync();
                EnableReconnection = false;
            };

            SatPlaceClient.OnClosed += delegate
            {
                ConnectionReady = false;
                EnableReconnection = true;
            };

            SatPlaceClient.OnError += delegate
            {
                ConnectionReady = false;
                EnableReconnection = true;
                DoTryConnecting();
            };
        }

        private void DoReconnect()
        {
            EnableReconnection = false;
            _retryCounter = 0;
            DoTryConnecting();
        }

        private void HandleGetSettingsResult(ResponseArgs args)
        {
            var raw1 = JsonConvert.DeserializeObject<GetSettingsPayloadResult>(args.Text);
            OrderSettings = new OrderSettingsResult(raw1.RawData);
        }

        private void HandleBroadcastHeartbeat(ResponseArgs args)
        {
            var result = JsonConvert.DeserializeObject<GenericPayloadResult>(args.Text.Trim());

        }

        private void HandleLatestCanvasData(ResponseArgs args)
        {
            var result = JsonConvert.DeserializeObject<GenericPayloadResult>(args.Text);

            var base64raw = (result.Data as string).Replace("data:image/bmp;base64,", string.Empty);

            var pngData = Convert.FromBase64String(base64raw);

            var canvasPng = Png.Open(pngData);

            var canvasBitmap = new GenericColor[canvasPng.Width * canvasPng.Height];

            for (int x = 0; x < canvasPng.Width; x++)
                for (int y = 0; y < canvasPng.Height; y++)
                {
                    var i = x + canvasPng.Width * y;
                    var pngPixel = canvasPng.GetPixel(x, y);
                    var newPixel = new GenericColor(pngPixel.R, pngPixel.G, pngPixel.B, pngPixel.A);
                    canvasBitmap[i] = newPixel;
                }

            LatestCanvasBitmap = new GenericBitmap(canvasPng.Width, canvasPng.Height, canvasBitmap);

            CanvasRefreshInProgress = false;
        }
    }
}