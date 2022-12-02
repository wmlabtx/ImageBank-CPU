using System;

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

        public int Year { get; private set; }

        public void SetActualYear()
        {
            Year = DateTime.Now.Year;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeYear, Year);
        }

        public DateTime LastView { get; private set; }

        public void SetLastView(DateTime lastview)
        {
            LastView = lastview;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeLastView, LastView);
        }

        public float[] _vector;

        public float[] GetVector()
        {
            return _vector;
        }

        public void SetVector(float[] vector)
        {
           _vector = vector;
            AppDatabase.ImageUpdateProperty(Hash, AppConsts.AttributeVector, Helper.ArrayFromFloat(_vector));
        }

        public Img(
            string name,
            string hash,
            int year,
            float[] vector,
            DateTime lastview
            )
        {
            Name = name;
            Hash = hash;
            Year = year;
            _vector = vector;
            LastView = lastview;
        }
    }
}