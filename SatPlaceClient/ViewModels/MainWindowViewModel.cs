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

        public MainWindowViewModel()
        {
             SatPlaceClient = new SocketIO("https://api.satoshis.place"); 

            OpenImageDialog = ReactiveCommand.CreateFromTask(async () =>
            {
                var client = new SocketIO("https://api.satoshis.place");

                client.OnConnected += async delegate
                {
                    client.On("GET_LATEST_PIXELS_RESULT", res =>
                    {
                        Console.WriteLine(res.Text);
                    });

                    client.On("BROADCAST_STATS", res =>
                    {
                        Console.WriteLine(res.Text);
                    });

                    await client.EmitAsync("GET_LATEST_PIXELS", null);

                };



                await client.ConnectAsync();
            });
        }
    }
}