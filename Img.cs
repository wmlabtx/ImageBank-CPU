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

        public int Counter { get; private set; }
        public void SetCounter(int counter)
        {
            Counter = counter;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeCounter, Counter);
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

        public string BestHash { get; private set; }
        public void SetBestHash(string besthash)
        {
            BestHash = besthash;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeBestHash, BestHash);
        }

        public float Distance { get; private set; }
        public void SetDistance(float distance)
        {
            Distance = distance;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeDistance, Distance);
        }

        public DateTime LastCheck { get; private set; }
        public void SetLastCheck(DateTime lastcheck)
        {
            LastCheck = lastcheck;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeLastCheck, LastCheck);
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
            int counter,
            byte[] vector,
            RotateFlipType orientation,
            DateTime lastview,
            string besthash,
            float distance,
            DateTime lastcheck
            )
        {
            Name = name;
            Hash = hash;
            Counter = counter;
            _vector = vector;
            Orientation = orientation;
            LastView = lastview;
            BestHash = besthash;
            Distance = distance;
            LastCheck = lastcheck;
        }
    }
}