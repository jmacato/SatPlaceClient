using System;
using System.Numerics;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Text;
using ReactiveUI;
using SatPlaceClient.ImageProcessing;

namespace SatPlaceClient.Models
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct GenericColor
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
        /// A struct that represents an ARGB color and is aligned as
        /// a BGRA bytefield in memory.
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        /// <param name="a">Alpha</param>
        public GenericColor(byte r, byte g, byte b, byte a = byte.MaxValue)
        {
            this.A = a;
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public GenericColor(IColorSpace color)
        {
            var conv = color.ToRgb();
            this.A = byte.MaxValue;
            this.R = (byte)conv.R;
            this.G = (byte)conv.G;
            this.B = (byte)conv.B;
        }

        public GenericColor(Vector3 rgba)
        {
            this.R = (byte)(rgba.X * byte.MaxValue);
            this.G = (byte)(rgba.Y * byte.MaxValue);
            this.B = (byte)(rgba.Z * byte.MaxValue);
            this.A = byte.MaxValue;
        }
 
        public GenericColor(Vector4 rgba)
        {
            this.R = (byte)(rgba.X * byte.MaxValue);
            this.G = (byte)(rgba.Y * byte.MaxValue);
            this.B = (byte)(rgba.Z * byte.MaxValue);
            this.A = (byte)(rgba.W * byte.MaxValue);
        }
 
        public Vector3 ToVector3()
        {
            var nR = (R / 255f);
            var nG = (G / 255f);
            var nB = (B / 255f);

            return new Vector3(nR, nG, nB);
        }

        public static GenericColor FromVector4(Vector4 rgba)
        {
            var nR = (byte)(rgba.X * byte.MaxValue);
            var nG = (byte)(rgba.Y * byte.MaxValue);
            var nB = (byte)(rgba.Z * byte.MaxValue);
            var nA = (byte)(rgba.W * byte.MaxValue);
            return new GenericColor(nR, nG, nB, nA);
        }

        public Vector4 ToVector4()
        {
            var nR = (R / 255f);
            var nG = (G / 255f);
            var nB = (B / 255f);
            var nA = (A / 255f);

            return new Vector4(nR, nG, nB, nA);
        }

        public Rgb ToRgb()
        {
            return new Rgb(R, G, B);
        }

        public GenericColor AlphaBlend(GenericColor ForegroundPixel)
        {
            var normB = this.ToVector4();
            var normF = ForegroundPixel.ToVector4();

            var alpha = normF.W;
            var oneminusalpha = 1 - alpha;

            var oX = ((normF.X * alpha) + (oneminusalpha * normB.X));
            var oY = ((normF.Y * alpha) + (oneminusalpha * normB.Y));
            var oZ = ((normF.Z * alpha) + (oneminusalpha * normB.Z));

            return GenericColor.FromVector4(new Vector4(oX, oY, oZ, 1));
        }
    }
}
