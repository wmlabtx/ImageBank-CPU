using System;
using System.Drawing;

namespace ImageBank
{
    public class Img
    {
        public string Hash { get; }
        public string Folder { get; }

        public DateTime DateTaken { get; private set; }
        public void SetDateTaken(DateTime datetaken)
        {
            DateTaken = datetaken;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeDateTaken, DateTaken);
        }

        public float[] _histogram;
        public float[] GetHistogram()
        {
            return _histogram;
        }

        public void SetHistogram(float[] histogram)
        {
            _histogram = histogram;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeHistogram, Helper.ArrayFromFloat(_histogram));
        }

        public byte[] _vector;
        public byte[] GetVector()
        {
            return _vector;
        }

        public void SetVector(byte[] vector)
        {
            _vector = vector;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeVector, _vector);
        }

        public DateTime LastView { get; private set; }
        public void SetLastView(DateTime lastview)
        {
            LastView = lastview;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeLastView, LastView);
        }

        public RotateFlipType Orientation { get; private set; }
        public void SetOrientation(RotateFlipType rft)
        {
            Orientation = rft;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeOrientation, Helper.RotateFlipTypeToByte(Orientation));
        }

        public string BestHash { get; private set; }
        public void SetBestHash(string besthash)
        {
            BestHash = besthash;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeBestHash, BestHash);
        }

        public string GetFileName()
        {
            return $"{AppConsts.PathHp}\\{Folder[0]}\\{Folder[1]}\\{Hash}{AppConsts.MzxExtension}";
        }

        public string GetShortFileName()
        {
            return $"{Folder}\\{Hash.Substring(0, 4)}.{Hash.Substring(4, 4)}.{Hash.Substring(8, 4)}";
        }

        public Img(
        string hash,
            string folder,
            DateTime datetaken,
            float[] histogram,
            byte[] vector,
            DateTime lastview,
            RotateFlipType orientation,
            string besthash
            )
        {
            Hash = hash;
            Folder = folder;
            DateTaken = datetaken;
            _histogram = histogram;
            _vector = vector;
            Orientation = orientation;
            LastView = lastview;
            BestHash = besthash;
        }
    }
}