﻿//Note: This is a generated file.
using SatPlaceClient.ImageProcessing.Conversions;

namespace SatPlaceClient.ImageProcessing
{
    public interface IRgb : IColorSpace
    {

        double R { get; set; }

        double G { get; set; }

        double B { get; set; }
    }

    public class Rgb : ColorSpace, IRgb
    {
        public Rgb()
        {
        }

        public Rgb(double R, double G, double B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }

        public double R { get; set; }

        public double G { get; set; }

        public double B { get; set; }

        public override void Initialize(IRgb color)
        {
            RgbConverter.ToColorSpace(color, this);
        }

        public override IRgb ToRgb()
        {
            return RgbConverter.ToColor(this);
        }
    }

    public interface IXyz : IColorSpace
    {

        double X { get; set; }

        double Y { get; set; }

        double Z { get; set; }

    }

    public class Xyz : ColorSpace, IXyz
    {

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }


        public override void Initialize(IRgb color)
        {
            XyzConverter.ToColorSpace(color, this);
        }

        public override IRgb ToRgb()
        {
            return XyzConverter.ToColor(this);
        }
    }

    public interface IHsl : IColorSpace
    {

        double H { get; set; }

        double S { get; set; }

        double L { get; set; }

    }
 
    public interface ILab : IColorSpace
    {

        double L { get; set; }

        double A { get; set; }

        double B { get; set; }

    }

    public class Lab : ColorSpace, ILab
    {

        public double L { get; set; }

        public double A { get; set; }

        public double B { get; set; }


        public override void Initialize(IRgb color)
        {
            LabConverter.ToColorSpace(color, this);
        }

        public override IRgb ToRgb()
        {
            return LabConverter.ToColor(this);
        }
    }

    public interface ILch : IColorSpace
    {

        double L { get; set; }

        double C { get; set; }

        double H { get; set; }

    }

    public class Lch : ColorSpace, ILch
    {

        public double L { get; set; }

        public double C { get; set; }

        public double H { get; set; }


        public override void Initialize(IRgb color)
        {
            LchConverter.ToColorSpace(color, this);
        }

        public override IRgb ToRgb()
        {
            return LchConverter.ToColor(this);
        }
    }


    public interface ILuv : IColorSpace
    {

        double L { get; set; }

        double U { get; set; }

        double V { get; set; }

    }

    public class Luv : ColorSpace, ILuv
    {

        public double L { get; set; }

        public double U { get; set; }

        public double V { get; set; }


        public override void Initialize(IRgb color)
        {
            LuvConverter.ToColorSpace(color, this);
        }

        public override IRgb ToRgb()
        {
            return LuvConverter.ToColor(this);
        }
    }


    public interface IYxy : IColorSpace
    {

        double Y1 { get; set; }

        double X { get; set; }

        double Y2 { get; set; }

    }

    public class Yxy : ColorSpace, IYxy
    {

        public double Y1 { get; set; }

        public double X { get; set; }

        public double Y2 { get; set; }


        public override void Initialize(IRgb color)
        {
            YxyConverter.ToColorSpace(color, this);
        }

        public override IRgb ToRgb()
        {
            return YxyConverter.ToColor(this);
        }
    }
}