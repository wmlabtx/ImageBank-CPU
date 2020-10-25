using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageBank
{
    public class ImageRGB : IDisposable
    {
        private bool _disposed = false;

        private readonly Bitmap _bitmap;
        private readonly BitmapData _bitmapdata;
        private readonly int _stride;
        private readonly int _size;
        private int[] fastbitx;
        private int[] fastbytex;
        private int[] fasty;

        public int Width { get; }
        public int Height { get; }

        ~ImageRGB() {
            Dispose(false);
        }

        public ImageRGB(Image image) {
            Contract.Requires(image != null);
            _bitmap = (Bitmap)image;
            Width = _bitmap.Width;
            Height = _bitmap.Height;
            var bounds = Rectangle.FromLTRB(0, 0, Width, Height);
            _bitmapdata = _bitmap.LockBits(bounds, ImageLockMode.ReadOnly, _bitmap.PixelFormat);
            _stride = _bitmapdata.Stride < 0 ? -_bitmapdata.Stride : _bitmapdata.Stride;
            _size = _stride * Height;
            fastbitx = new int[Width];
            fastbytex = new int[Width];
            fasty = new int[Height];
            for (var x = 0; x < Width; x++) {
                fastbitx[x] = x * 24;
                fastbytex[x] = fastbitx[x] >> 3;
                fastbitx[x] = fastbitx[x] % 8;
            }

            for (var y = 0; y < Height; y++) {
                fasty[y] = y * _bitmapdata.Stride;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) {
                return;
            }

            if (disposing) {
                _bitmap.UnlockBits(_bitmapdata);
                _bitmap.Dispose();
            }

            fastbitx = null;
            fastbytex = null;
            fasty = null;

            _disposed = true;
        }
    }
}
