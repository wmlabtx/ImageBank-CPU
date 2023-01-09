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

        public static void ToggleXor()
        {
            if (_imgpanels[0].Bitmap.Width == _imgpanels[1].Bitmap.Width && _imgpanels[0].Bitmap.Height == _imgpanels[1].Bitmap.Height) {
                var bitmapxor = BitmapHelper.BitmapXor(_imgpanels[0].Bitmap, _imgpanels[1].Bitmap);
                _imgpanels[1].SetBitmap(bitmapxor);
            }
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
        private static List<string> _similars;

        public static void SetSimilars(List<string> similars, IProgress<string> progress)
        {
            _similars = similars;
            SetFirstPosition(progress);
        }

        public static string GetRightName()
        {
            var hash = _similars[_position];
            if (!AppImgs.TryGetValue(hash, out Img img)) {
                return null;
            }

            return img.Name;
        }

        public static void SetFirstPosition(IProgress<string> progress)
        {
            _position = 0;
            while (!SetImgPanel(1, _similars[_position])) {
                _similars.RemoveAt(0);
            }

            UpdateStatus(progress);
        }

        public static void SetLastPosition(IProgress<string> progress)
        {
            _position = _similars.Count - 1;
            while (!SetImgPanel(1, _similars[_position])) {
                _similars.RemoveAt(_position);
                _position--;
            }

            UpdateStatus(progress);
        }

        public static void MoveRightPosition(IProgress<string> progress)
        {
            while (_position < _similars.Count - 1) {
                _position++;
                if (SetImgPanel(1, _similars[_position])) {
                    UpdateStatus(progress);
                    break;
                }
            }
        }

        public static void MoveLeftPosition(IProgress<string> progress)
        {
            while (_position > 0) {
                _position--;
                if (SetImgPanel(1, _similars[_position])) {
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
            progress?.Report($"{totalcount}: {imgX.Name} [{age} ago] = ({_position}/{similarsfound})");
        }
    }
}
