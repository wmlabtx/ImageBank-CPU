using System;
using System.Linq;

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

        public float Distance { get; private set; }

        public void SetDistance(float distance)
        {
            Distance = distance;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeDistance, Distance);
        }

        private int[] _ni;
        public int[] GetHistory()
        {
            return _ni;
        }

        public int GetHistorySize()
        {
            return _ni.Length;
        }

        public void SetHistory(int[] array)
        {
            _ni = array;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeNi, Helper.ArrayFrom32(_ni));
        }

        public void AddHistory(int next)
        {
            if (InHistory(next)) {
                return;
            }

            var list = _ni.ToList();
            list.Add(next);
            while (list.Count > 10) {
                list.RemoveAt(0);
            }

            _ni = list.ToArray();
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeNi, Helper.ArrayFrom32(_ni));
        }

        public void RemoveRank(int next)
        {
            if (!InHistory(next)) {
                return;
            }

            var list = _ni.ToList();
            list.Remove(next);
            _ni = list.ToArray();
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeNi, Helper.ArrayFrom32(_ni));
        }

        public bool InHistory(int next)
        {
            return _ni.Contains(next);
        }

        public int FamilyId { get; private set; }

        public void SetFamilyId(int familyid)
        {
            if (FamilyId != familyid) {
                FamilyId = familyid;
                AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeFamilyId, familyid);
            }
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
            int[] ni,
            int familyid
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
            _ni = ni;
            FamilyId = familyid;
        }
    }
}