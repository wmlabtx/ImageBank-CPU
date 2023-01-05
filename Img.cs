using System;
using System.Drawing;

namespace ImageBank
{
    public class Img
    {
        public string Hash { get; }

        public string Name { get; private set; }
        public void SetName(string name)
        {
            Name = name;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeName, Name);
        }

        public DateTime LastView { get; private set; }
        public void SetLastView(DateTime lastview)
        {
            LastView = lastview;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeLastView, LastView);
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

        public Img(
            string name,
            string hash,
            byte[] vector,
            RotateFlipType orientation,
            DateTime lastview
            )
        {
            Name = name;
            Hash = hash;
            _vector = vector;
            Orientation = orientation;
            LastView = lastview;
        }
    }
}