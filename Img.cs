using System;
using System.Drawing;

namespace ImageBank
{
    public class Img
    {
        public string Hash { get; }
        public string Name { get; }

        public DateTime LastView { get; private set; }
        public void SetLastView(DateTime lastview)
        {
            LastView = lastview;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeLastView, LastView);
        }

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
        public byte[] GetVector() {
            return _vector;
        }

        public void SetVector(byte[] vector)
        {
           _vector = vector;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeVector, _vector);
        }

        public RotateFlipType Orientation { get; private set; }
        public void SetOrientation(RotateFlipType rft)
        {
            Orientation = rft;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeOrientation, Helper.RotateFlipTypeToByte(Orientation));
        }

        public string Family { get; private set; }
        public void SetFamily(string family)
        {
            Family = family;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeFamily, Family);
        }

        public Img(
            string name,
            string hash,
            DateTime datetaken,
            float[] histogram,
            byte[] vector,
            RotateFlipType orientation,
            string family,
            DateTime lastview
            )
        {
            Name = name;
            Hash = hash;
            DateTaken = datetaken;
            _histogram = histogram;
            _vector = vector;
            Orientation = orientation;
            LastView = lastview;
            Family = family;
        }
    }
}