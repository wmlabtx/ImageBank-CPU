using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public string Name { get; }
        public string Hash { get; }

        private float[] _vector;
        public float[] GetVector()
        {
            return _vector;
        }
       
        public void SetVector(float[] vector)
        {
            _vector = vector;
            var buffer = Helper.ArrayFromFloat(vector);
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeVector, buffer);
        }

        public int Year { get; private set; }

        public void SetActualYear()
        {
            Year = DateTime.Now.Year;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeYear, Year);
        }

        public int BestId { get; private set; }

        public void SetBestId(int bestid)
        {
            BestId = bestid;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeBestId, BestId);
        }

        public DateTime LastView { get; private set; }

        public void SetLastView(DateTime lastview)
        {
            LastView = lastview;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeLastView, LastView);
        }

        public DateTime LastCheck { get; private set; }

        public void SetLastCheck(DateTime lastcheck)
        {
            LastCheck = lastcheck;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeLastCheck, LastCheck);
        }

        public float Distance { get; private set; }

        public void SetDistance(float distance)
        {
            Distance = distance;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeDistance, Distance);
        }

        public Img(
            int id,
            string name,
            string hash,
            float[] vector,
            float distance,
            int year,
            int bestid,
            DateTime lastview,
            DateTime lastcheck
        )
        {
            Id = id;
            Name = name;
            Hash = hash;
            _vector = vector;
            Year = year;
            BestId = bestid;
            Distance = distance;
            LastView = lastview;
            LastCheck = lastcheck;
        }
    }
}