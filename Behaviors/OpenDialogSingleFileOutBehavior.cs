using System;
using System.Collections.Generic;
using System.IO; 
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace SatPlaceClient.Behaviors
{
    public class OpenDialogSingleFileOutBehavior : Behavior<Button>
    {
        public static readonly AvaloniaProperty<string> TargetFilePathProperty =
            AvaloniaProperty.Register<OpenDialogSingleFileOutBehavior, string>(nameof(TargetFilePath), defaultBindingMode: BindingMode.TwoWay);

        public string TargetFilePath
        {
            get => GetValue(TargetFilePathProperty);
            set => SetValue(TargetFilePathProperty, value);
        }

        public static readonly AvaloniaProperty<string> DialogTitleProperty =
            AvaloniaProperty.Register<OpenDialogSingleFileOutBehavior, string>(nameof(DialogTitle));

        public string DialogTitle
        {
            get => GetValue(DialogTitleProperty);
            set => SetValue(DialogTitleProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += delegate { Dispatcher.UIThread.Post(Execute); };
        }

        private async void Execute()
        {
            var dialog = new OpenFileDialog()
            {
                Title = DialogTitle,
                AllowMultiple = false,
                Filters =
                {
                    new FileDialogFilter()
                    {
                        Extensions =
                        {
                            "png",
                            "PNG",
                        },
                        Name = "Portable Network Graphics Files (*.PNG)"
                    }
                }
            };

            var result = await dialog.ShowAsync(GetWindow());

            if (result is null) return;
            else TargetFilePath = result[0];
        }

        private Window GetWindow()
        {
            return (Window)((IControl)this.AssociatedObject).VisualRoot;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
