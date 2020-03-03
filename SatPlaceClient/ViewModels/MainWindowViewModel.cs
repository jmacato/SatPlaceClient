using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Reactive;
using System.Text;
using ReactiveUI;
using SocketIOClient;

namespace SatPlaceClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string OpenImageDialogText { get; set; } = "Open Image";

        public ReactiveCommand<Unit, Unit> OpenImageDialog { get; }

        private SocketIO SatPlaceClient { get; }

        private object _connectionReadyLock = new object();
        private bool _connectionReady;

        public bool ConnectionReady
        {
            get
            {
                lock (_connectionReadyLock)
                    return _connectionReady;
            }
            set
            {
                lock (_connectionReadyLock)
                    this.RaiseAndSetIfChanged(ref _connectionReady, value, nameof(ConnectionReady));
            }
        }

        public MainWindowViewModel()
        {
            SatPlaceClient = new SocketIO("https://api.satoshis.place");

            SetupHandlers();


            OpenImageDialog = ReactiveCommand.CreateFromTask(async () =>
            {
                if (ConnectionReady)
                    await SatPlaceClient.EmitAsync("GET_LATEST_PIXELS", null);
            });
        }

        private async void SetupHandlers()
        {
            await SatPlaceClient.ConnectAsync();

            SatPlaceClient.OnConnected += delegate
            {
                ConnectionReady = true;

                SatPlaceClient.On("GET_LATEST_PIXELS_RESULT", res =>
                {
                    Console.WriteLine(res.Text);
                });

                SatPlaceClient.On("BROADCAST_STATS", res =>
                {
                    Console.WriteLine(res.Text);
                });
            };

            SatPlaceClient.OnClosed += delegate
            {
                ConnectionReady = false;
            };

            SatPlaceClient.OnError += delegate
            {
                ConnectionReady = false;
            };
        }
    }
}