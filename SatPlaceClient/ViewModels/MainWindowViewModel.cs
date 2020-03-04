using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using Newtonsoft.Json;
using ReactiveUI;
using SocketIOClient;
using SatPlaceClient.Models.Json;
using System.Buffers;
using SatPlaceClient.Models;
using System.Threading.Tasks;
using System.Reactive.Linq;
using SocketIOClient.Arguments;

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
        }

        private SocketIO SatPlaceClient { get; }
        private GenericPixel[] _latestCanvasBitmap;
        private object lockObj = new object();
        private bool _connectionReady;
        private bool _canvasRefreshInProgress;
        private byte RetryCounter;
        private bool _EnableReconnection;

        /// <summary>
        /// Stores the latest bitmap data of satoshis.place's canvas.
        /// </summary>
        public GenericPixel[] LatestCanvasBitmap
        {
            get => _latestCanvasBitmap;
            set => this.RaiseAndSetIfChanged(ref _latestCanvasBitmap, value, nameof(LatestCanvasBitmap));
        }

        public bool ConnectionReady
        {
            get
            {
                lock (lockObj)
                    return _connectionReady;
            }
            set
            {
                lock (lockObj)
                    this.RaiseAndSetIfChanged(ref _connectionReady, value, nameof(ConnectionReady));
            }
        }

        public bool CanvasRefreshInProgress
        {
            get
            {
                lock (lockObj)
                {
                    return _canvasRefreshInProgress;
                }
            }

            private set
            {
                lock (lockObj)
                {
                    this.RaiseAndSetIfChanged(ref _canvasRefreshInProgress, value, nameof(CanvasRefreshInProgress));
                }
            }
        }

        public bool EnableReconnection
        {
            get => _EnableReconnection;
            private set => this.RaiseAndSetIfChanged(ref _EnableReconnection, value, nameof(EnableReconnection));
        }

        public ReactiveCommand<Unit, Unit> RefreshCanvasCommand { get; }
        public ReactiveCommand<Unit, Unit> ReconnectCommand { get; }

        private async void DoTryConnecting()
        {
            try
            {
                await SatPlaceClient.ConnectAsync();
                RetryCounter = 0;
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, RetryCounter)));
                RetryCounter++;

                if (RetryCounter <= 2)
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
            RetryCounter = 0;
            DoTryConnecting();
        }
        
        private void HandleGetSettingsResult(ResponseArgs args)
        {
            var raw1 = JsonConvert.DeserializeObject<GetSettingsPayloadResult>(args.Text);
            var result = new SettingsResult(raw1.RawData);
        }

        private void HandleBroadcastHeartbeat(ResponseArgs args)
        {

        }

        private void HandleLatestCanvasData(ResponseArgs args)
        {
            var result = JsonConvert.DeserializeObject<GenericPayloadResult>(args.Text);

            var base64raw = result.Data.Replace("data:image/bmp;base64,", string.Empty);

            var pngData = Convert.FromBase64String(base64raw);

            var canvasPng = BigGustave.Png.Open(pngData);

            var canvasBitmap = new GenericPixel[1000 * 1000];

            for (int x = 0; x < 1000; x++)
                for (int y = 0; y < 1000; y++)
                {
                    var i = x + 1000 * y;
                    var pngPixel = canvasPng.GetPixel(x, y);
                    var newPixel = new GenericPixel(pngPixel.R, pngPixel.G, pngPixel.B, pngPixel.A);
                    canvasBitmap[i] = newPixel;
                }

            LatestCanvasBitmap = canvasBitmap;

            CanvasRefreshInProgress = false;
        }
    }
}