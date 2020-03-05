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
using SatPlaceClient.ImageProcessing.Dithering;

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

            _addImageEnabled = this.WhenAnyValue(x => x.CanvasRefreshInProgress,
                                                     x => x.ConnectionReady,
                                                     x => x.TargetImage)
                                                           .Select(x => !x.Item1 & x.Item2 & x.Item3 == null)
                                                           .ObserveOn(RxApp.MainThreadScheduler)
                                                           .ToProperty(this, x => x.AddImageEnabled);

            var CanRemoveImage = this.WhenAnyValue(x => x.AddImageEnabled, x => x.TargetImage)
                                       .Select(x => !x.Item1 & x.Item2 != null)
                                       .ObserveOn(RxApp.MainThreadScheduler);

            RefreshCanvasCommand = ReactiveCommand.CreateFromTask(DoRefreshCanvasAsync, CanRefreshCanvas);

            ReconnectCommand = ReactiveCommand.Create(DoReconnect);

            RemoveImageCommand = ReactiveCommand.Create(DoRemoveImage, CanRemoveImage);

            this.WhenAnyValue(x => x.TargetImageFilePath)
                .Where(x => x != null)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(ImageFileOpened);
        }

        private void DoRemoveImage()
        {
            TargetImage = null;
            TargetImageX = 0;
            TargetImageY = 0;
        }

        private SocketIO SatPlaceClient { get; }
        private GenericBitmap _latestCanvasBitmap, _targetImage;
        private OrderSettingsResult _orderSettings;

        private bool _connectionReady, _canvasRefreshInProgress, _enableReconnection, _pngFileProcessingInProgress;
        private byte _retryCounter;
        private string _targetImageFilePath, _errorMessage;
        public uint MaximumReconnectionAttempt { get; } = 3;

        /// <summary>
        /// Stores the latest bitmap data of satoshis.place's canvas.
        /// </summary>
        public GenericBitmap LatestCanvasBitmap
        {
            get => _latestCanvasBitmap;
            private set => this.RaiseAndSetIfChanged(ref _latestCanvasBitmap, value, nameof(LatestCanvasBitmap));
        }

        /// <summary>
        /// Stores the bitmap data of the target image to upload.
        /// </summary>
        public GenericBitmap TargetImage
        {
            get => _targetImage;
            private set => this.RaiseAndSetIfChanged(ref _targetImage, value, nameof(TargetImage));
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


        private ObservableAsPropertyHelper<bool> _addImageEnabled;
        public bool AddImageEnabled => _addImageEnabled.Value;

        private double _targetImageX;

        public double TargetImageX
        {
            get => _targetImageX;
            set => this.RaiseAndSetIfChanged(ref _targetImageX, value, nameof(TargetImageX));
        }

        private double _targetImageY;

        public double TargetImageY
        {
            get => _targetImageY;
            set => this.RaiseAndSetIfChanged(ref _targetImageY, value, nameof(TargetImageY));
        }

        public string TargetImageFilePath
        {
            get => _targetImageFilePath;
            set => this.RaiseAndSetIfChanged(ref _targetImageFilePath, value, nameof(TargetImageFilePath));
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
        public ReactiveCommand<Unit, Unit> RemoveImageCommand { get; }

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

            var newDither = new AdobePatternDithering();

            int CoordsToIndex(int x, int y) => x + tW * y;

            for (int y = 0; y < tH; y++)
                for (int x = 0; x < tW; x++)
                {
                    var pngPixel = target.GetPixel(x, y);
                    newTargetImage.Pixels[CoordsToIndex(x, y)] = new GenericColor(pngPixel.R, pngPixel.G, pngPixel.B, byte.MaxValue);
                }

            newDither.Dither(tW, tH, ref newTargetImage.Pixels, OrderSettings.Colors);

            TargetImage = newTargetImage;
        }


        private async void DoTryConnecting()
        {
            try
            {
                await SatPlaceClient.ConnectAsync();

            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, _retryCounter)));
                _retryCounter++;
                DisplayError($"Attempting to reconnect... ({_retryCounter}/{MaximumReconnectionAttempt})");

                if (_retryCounter <= MaximumReconnectionAttempt)
                {
                    DoTryConnecting();
                }
                else
                {
                    DisplayError($"Reconnection failed. Press the \"Reconnect\" button to try again.");
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
                _retryCounter = 0;
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