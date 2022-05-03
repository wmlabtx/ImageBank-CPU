using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public string Name { get; }
        public string Hash { get; }

        private float[] _histogram;
        public float[] GetHistogram()
        {
            return _histogram;
        }

        public void SetHistogram(float[] histogram)
        {
            _histogram = histogram;
            var buffer = Helper.ArrayFromFloat(histogram);
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeHistogram, buffer);
        }

        public int Year { get; private set; }

        public void SetActualYear()
        {
            Year = DateTime.Now.Year;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeYear, Year);
        }

        public int Counter => GetHistorySize();

        private int _bestid;
        public int BestId {
            get => _bestid;
            set {
                _bestid = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeBestId, value);
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

        private int[] _history;

        public int[] GetHistory()
        {
            return _history;
        }

        public int GetHistorySize()
        {
            return _history.Length;
        }

        public bool IsInHistory(int id)
        {
            var pos = Array.BinarySearch(_history, id);
            return (pos >= 0);
        }

        public void AddToHistory(int id)
        {
            if (IsInHistory(id)) {
                return;
            }

            var result = new int[_history.Length + 1];
            _history.CopyTo(result, 0);
            result[_history.Length] = id;
            _history = result;
            Array.Sort(_history);
            var buffer = Helper.ArrayFrom32(_history);
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeHistory, buffer);
        }

        public void RemoveFromHistory(int id)
        {
            var pos = Array.BinarySearch(_history, id);
            if (pos < 0) {
                return;
            }

            var result = new int[_history.Length - 1];
            if (pos > 0) {
                Array.Copy(_history, 0, result, 0, pos);
            }

            if (pos < _history.Length - 1) {
                Array.Copy(_history, pos + 1, result, pos, _history.Length - pos - 1);
            }

            _history = result;
            var buffer = Helper.ArrayFrom32(_history);
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeHistory, buffer);
        }

        public Img(
            int id,
            string name,
            string hash,
            float[] histogram,
            int year,
            int bestid,
            DateTime lastview,
            DateTime lastcheck,
            int[] history
        )
        {
            Id = id;
            Name = name;
            Hash = hash;
            _histogram = histogram;
            Year = year;
            _bestid = bestid;
            LastView = lastview;
            LastCheck = lastcheck;
            _history = history;
        }
    }
}