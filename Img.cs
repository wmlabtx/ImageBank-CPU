using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public string Name { get; }
        public string Hash { get; }

        private ulong[][] _fingerprints;
        public ulong[][] Fingerprints
        {
            get => _fingerprints;
            set
            {
                _fingerprints = value;
                var buffer = Helper.ArrayFrom64(_fingerprints[0]);
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeFp, buffer);
                var bufferflip = Helper.ArrayFrom64(_fingerprints[1]);
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeFpFlip, bufferflip);
            }
        }

        public int Year { get; private set; }

        public void SetActualYear()
        {
            Year = DateTime.Now.Year;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeYear, Year);
        }

        public int Counter { get; private set; }

        public void IncreaseCounter()
        {
            Counter++;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeCounter, Counter);
        }

        public void ResetCounter()
        {
            Counter = 0;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeCounter, Counter);
        }

        private int _bestid;
        public int BestId {
            get => _bestid;
            set {
                _bestid = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeBestId, value);
            }
        }

        private float _bestvdistance;
        public float BestVDistance {
            get => _bestvdistance;
            set {
                _bestvdistance = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeBestVDistance, value);
            }
        }

        public DateTime LastView { get; private set; }

        public void SetLastView()
        {
            LastView = DateTime.Now;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeLastView, LastView);
        }

        public DateTime LastCheck { get; private set; }

        public void SetLastCheck()
        {
            LastCheck = DateTime.Now;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeLastCheck, LastCheck);
        }

        public Img(
            int id,
            string name,
            string hash,
            ulong[][] fingerprints,
            int year,
            int counter,
            int bestid,
            float bestvdistance,
            DateTime lastview,
            DateTime lastcheck 
        )
        {
            Id = id;
            Name = name;
            Hash = hash;
            _fingerprints = fingerprints;
            Year = year;
            Counter = counter;
            _bestid = bestid;
            _bestvdistance = bestvdistance;
            LastView = lastview;
            LastCheck = lastcheck;
        }
    }
}