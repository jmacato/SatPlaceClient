using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using ReactiveUI;

namespace SatPlaceClient.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        private volatile bool isDisposed;
        public CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public void Dispose()
        {
            if (!isDisposed)
            {
                Disposables?.Dispose();
                isDisposed = true;
            }
        }
    }
}
