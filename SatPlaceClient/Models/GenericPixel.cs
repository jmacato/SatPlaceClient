using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Text;
using ReactiveUI;

namespace SatPlaceClient.Models
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct GenericPixel 
    {
        [FieldOffset(0)]
        public readonly byte B;

        [FieldOffset(1)]
        public readonly byte G;

        [FieldOffset(2)]
        public readonly byte R;
        
        [FieldOffset(3)]
        public readonly byte A;

        /// <summary>
        /// A struct that represents a ARGB color and is aligned as
        /// a BGRA bytefield in memory.
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        public GenericPixel(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }
 
    }
}
