using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageBank
{
    /*
    Order	Path
    1	/app1/ifd/exif/{ushort=36867}
    2	/app13/irb/8bimiptc/iptc/date created
    3	/xmp/xmp:CreateDate
    4	/app1/ifd/exif/{ushort=36868}
    5	/app13/irb/8bimiptc/iptc/date created
    6	/xmp/exif:DateTimeOriginal
    */

    public static class MetadataHelper
    {
        private static readonly StringBuilder _sb = new StringBuilder();
        private static int _tagscounter;
        private static DateTime? _datetaken;

        private static void CaptureMetadata(ImageMetadata imageMetadata, string query)
        {
            try {
                if (imageMetadata is BitmapMetadata bitmapMetadata) {
                    foreach (string relativeQuery in bitmapMetadata) {
                        string fullQuery = query + relativeQuery;
                        var metadataQueryReader = bitmapMetadata.GetQuery(relativeQuery);
                        if (metadataQueryReader is string rawstring) {
                            //Console.WriteLine($"[{fullQuery}] {rawstring}");
                            if (rawstring.Length >= 10) {
                                var date = rawstring.Substring(0, 10);
                                if (DateTime.TryParseExact(date, "yyyy:MM:dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt) ||
                                    DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) {
                                    _sb.AppendLine($"[{fullQuery}] {rawstring}");
                                    if (dt.Year >= 1990 && dt < DateTime.Now) {
                                        if (_datetaken == null || dt < _datetaken) {
                                            _datetaken = dt;
                                        }
                                    }
                                }
                            }
                        }

                        if (metadataQueryReader is BitmapMetadata innerBitmapMetadata) {
                            CaptureMetadata(innerBitmapMetadata, fullQuery);
                        }
                    }
                }
            }
            catch (FileFormatException) {
            }
            catch (ArgumentException) {
            }
        }

        public static void GetMetadata(byte[] imagedata, out DateTime? datataken, out string metadata)
        {
            _sb.Clear();
            _tagscounter = 0;
            _datetaken = null;
            datataken = null;
            metadata = string.Empty;
            if (imagedata == null) {
                return;
            }

            using (var ms = new MemoryStream(imagedata)) {
                BitmapDecoder decoder;
                try {
                    decoder = BitmapDecoder.Create((Stream)ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None);
                }
                catch (NotSupportedException) {
                    return;
                }

                CaptureMetadata(decoder.Frames[0].Metadata, string.Empty);
            }

            datataken = _datetaken;
            if (_tagscounter > 0) {
                _sb.Append($"Exif tags: {_tagscounter}");
            }

            if (_sb.Length > 0) {
                metadata = _sb.ToString();
            }
        }
    }
}
