using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using QRCoder;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace SatPlaceClient.Controls
{
    public class QrCode : Control
    {
        private const int MatrixPadding = 2;
        private Size CoercedSize { get; set; }
        private double GridCellFactor { get; set; }
        private bool[,] FinalMatrix { get; set; }
        private IBrush SemiTransparentWhite = new SolidColorBrush(Colors.White, 0.5f).ToImmutable();
        public static readonly DirectProperty<QrCode, string> DataProperty =
       AvaloniaProperty.RegisterDirect<QrCode, string>(
           nameof(Data),
           o => o.Data,
           (o, v) => o.Data = v);

        private string _Data;

        public string Data
        {
            get { return _Data; }
            set { SetAndRaise(DataProperty, ref _Data, value); }
        }
        static QrCode()
        {
            AffectsMeasure<QrCode>(DataProperty);
        }

        public QrCode()
        {
            CoercedSize = new Size();

            this.WhenAnyValue(x => x.Data)
                .Where(x => !string.IsNullOrEmpty(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(DataToMatrix);
        }

        private void DataToMatrix(string dataStr)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(dataStr, QRCodeGenerator.ECCLevel.L);
            var srcMatrix = qrCodeData.ModuleMatrix;
            var qrDims = srcMatrix.Count;

            bool[,] tempMatrix = new bool[qrDims, qrDims];

            for (int y = 0; y < qrDims; y++)
                for (int x = 0; x < qrDims; x++)
                {
                    tempMatrix[x, y] = srcMatrix[x][y];
                }

            FinalMatrix = AddPaddingToMatrix(tempMatrix);
        }

        private bool[,] AddPaddingToMatrix(bool[,] matrix)
        {
            var dims = GetMatrixDimensions(matrix);
            var nW = dims.w + (MatrixPadding * 2);
            var nH = dims.h + (MatrixPadding * 2);

            var paddedMatrix = new bool[nH, nW];

            for (var i = 0; i < dims.w; i++)
                for (var j = 0; j < dims.h; j++)
                {
                    paddedMatrix[i + MatrixPadding, j + MatrixPadding] = matrix[i, j];
                }

            return paddedMatrix;
        }

        public override void Render(DrawingContext context)
        {
            var source = FinalMatrix;

            if (source is null)
            {
                return;
            }

            var dims = GetMatrixDimensions(source);

            context.FillRectangle(SemiTransparentWhite, new Rect(0, 0, GridCellFactor * dims.w, GridCellFactor * dims.h));

            for (var i = 0; i < dims.h; i++)
            {
                for (var j = 0; j < dims.w; j++)
                {
                    var cellValue = source[i, j];
                    var rect = new Rect(i * GridCellFactor, j * GridCellFactor, GridCellFactor, GridCellFactor);
                    var color = cellValue ? Brushes.Black : Brushes.Transparent;
                    context.FillRectangle(color, rect);
                }
            }
        }

        private (int w, int h) GetMatrixDimensions(bool[,] source)
        {
            return (source.GetUpperBound(0) + 1, source.GetUpperBound(1) + 1);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var source = FinalMatrix;

            if (source is null || source.Length == 0)
            {
                return new Size();
            }

            var dims = GetMatrixDimensions(source);
            var minDimension = Math.Min(dims.w, dims.h);
            var availMax = Math.Min(availableSize.Width, availableSize.Height);

            GridCellFactor = Math.Floor(availMax / minDimension);

            var maxF = Math.Min(availMax, GridCellFactor * minDimension);

            CoercedSize = new Size(maxF, maxF);

            return CoercedSize;
        }
    }
}
