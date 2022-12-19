using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;

namespace ImageBank
{
    public static class AppPanels
    {
        private static readonly ImgPanel[] _imgpanels = new ImgPanel[2];

        public static ImgPanel GetImgPanel(int idpanel)
        {
            return _imgpanels[idpanel];
        }

        public static bool SetImgPanel(int idpanel, string hash)
        {
            if (!AppImgs.TryGetValue(hash, out Img img)) {
                return false;
            }

            var filename = FileHelper.NameToFileName(img.Name);
            var lastmodified = File.GetLastWriteTime(filename);
            var imagedata = FileHelper.ReadFile(filename);
            if (imagedata == null) {
                return false;
            }

            using (var magickImage = BitmapHelper.ImageDataToMagickImage(imagedata)) {
                if (magickImage == null) {
                    var badname = Path.GetFileName(filename);
                    var badfilename = $"{AppConsts.PathGb}\\{badname}{AppConsts.CorruptedExtension}";
                    if (File.Exists(badfilename)) {
                        FileHelper.DeleteToRecycleBin(badfilename);
                    }

                    File.WriteAllBytes(badfilename, imagedata);
                    return false;
                }

                var format = magickImage.Format.ToString().ToLower();
                var datetaken = BitmapHelper.GetDateTaken(magickImage, lastmodified);
                var bitmap = BitmapHelper.MagickImageToBitmap(magickImage, img.Orientation);
                if (bitmap != null) {
                    /*
                    if (idpanel== 1) { 
                        if (_imgpanels[0].Bitmap.Width == bitmap.Width && _imgpanels[0].Bitmap.Height == bitmap.Height) {
                            var bitmapxor = BitmapHelper.BitmapXor(_imgpanels[0].Bitmap, bitmap);
                            bitmap.Dispose();
                            bitmap = bitmapxor;
                        }
                    }
                    */

                    var imgpanel = new ImgPanel(
                        img: img,
                        size: imagedata.LongLength,
                        bitmap: bitmap,
                        format: format,
                        datetaken: datetaken);

                    _imgpanels[idpanel] = imgpanel;
                }
            }

            return true;
        }

        private static int _position;
        private static List<Tuple<string, float>> _similars;

        public static void SetSimilars(List<Tuple<string, float>> similars, IProgress<string> progress)
        {
            _similars = similars;
            SetFirstPosition(progress);
        }

        public static void SetFirstPosition(IProgress<string> progress)
        {
            _position = 0;
            while (!SetImgPanel(1, _similars[_position].Item1)) {
                _similars.RemoveAt(0);
            }

            UpdateStatus(progress);
        }

        public static void SetLastPosition(IProgress<string> progress)
        {
            _position = _similars.Count - 1;
            while (!SetImgPanel(1, _similars[_position].Item1)) {
                _similars.RemoveAt(_position);
                _position--;
            }

            UpdateStatus(progress);
        }

        public static void MoveRightPosition(IProgress<string> progress)
        {
            while (_position < _similars.Count - 1) {
                _position++;
                if (SetImgPanel(1, _similars[_position].Item1)) {
                    UpdateStatus(progress);
                    break;
                }
            }
        }

        public static void MoveLeftPosition(IProgress<string> progress)
        {
            while (_position > 0) {
                _position--;
                if (SetImgPanel(1, _similars[_position].Item1)) {
                    UpdateStatus(progress);
                    break;
                }
            }
        }

        private static void UpdateStatus(IProgress<string> progress)
        {
            var totalcount = AppImgs.Count();
            var imgX = _imgpanels[0].Img;
            var age = Helper.TimeIntervalToString(DateTime.Now.Subtract(imgX.LastView));
            var similarsfound = _similars.Count;
            var distance = _similars[_position].Item2;
            progress?.Report($"{totalcount}: {imgX.Name} [{age} ago] = ({_position}/{similarsfound}) {distance:F2}");
        }
    }
}
