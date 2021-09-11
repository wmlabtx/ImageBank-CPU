using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public string Name { get; }
        public string Hash { get; }
        public DateTime? DateTaken { get; }
        public byte[] ColorHistogram { get; }

        private int _family;
        public int Family {
            get => _family;
            set {
                _family = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrFamily, value);
            }
        }

        public SortedList<int, int> History { get; }

        public void SaveHistory()
        {
            var history = History.Select(e => e.Key).ToArray();
            var buffer = new byte[history.Length * sizeof(int)];
            Buffer.BlockCopy(history, 0, buffer, 0, buffer.Length);
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrHistory, buffer);
        }

        private DateTime _lastview;
        public DateTime LastView {
            get => _lastview;
            set {
                _lastview = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrLastView, value);
            }
        }

        public Img(
            int id,
            string name,
            string hash,
            DateTime? datetaken,
            byte[] colorhistogram,
            int family,
            SortedList<int, int> history,
            DateTime lastview
            )
        {
            Id = id;
            Name = name;
            Hash = hash;
            DateTaken = datetaken;
            ColorHistogram = colorhistogram;
            _family = family;
            History = history;
            _lastview = lastview;
        }
    }
}