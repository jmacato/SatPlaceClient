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
        public static readonly DirectProperty<OpenDialogSingleFileOutBehavior, string> TargetFilePathProperty =
       AvaloniaProperty.RegisterDirect<OpenDialogSingleFileOutBehavior, string>(
           nameof(TargetFilePath),
           o => o.TargetFilePath,
           (o, v) => o.TargetFilePath = v);

        private string _TargetFilePath;

        public string TargetFilePath
        {
            get { return _TargetFilePath; }
            set { SetAndRaise(TargetFilePathProperty, ref _TargetFilePath, value); }
        }

        public static readonly DirectProperty<OpenDialogSingleFileOutBehavior, string> DialogTitleProperty =
       AvaloniaProperty.RegisterDirect<OpenDialogSingleFileOutBehavior, string>(
           nameof(DialogTitle),
           o => o.DialogTitle,
           (o, v) => o.DialogTitle = v);

        private string _DialogTitle;

        public string DialogTitle
        {
            get { return _DialogTitle; }
            set { SetAndRaise(DialogTitleProperty, ref _DialogTitle, value); }
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
