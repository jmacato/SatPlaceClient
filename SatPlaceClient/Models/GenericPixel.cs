using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using ReactiveUI;

namespace SatPlaceClient.Models
{
    public struct GenericPixel
    {
        public ushort A;
        public ushort R;
        public ushort G;
        public ushort B;

        public GenericPixel(ushort a, ushort r, ushort g, ushort b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }
    }
}
