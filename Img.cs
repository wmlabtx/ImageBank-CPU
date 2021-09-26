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
        public PHashEx PHashEx { get; }

        private int _year;
        public int Year {
            get => _year;
            set {
                _year = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrYear, value);
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

        private int _bestid;
        public int BestId {
            get => _bestid;
            set {
                _bestid = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrBestId, value);
            }
        }

        private int _bestdistance;
        public int BestDistance {
            get => _bestdistance;
            set {
                _bestdistance = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrBestDistance, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView {
            get => _lastview;
            set {
                _lastview = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrLastView, value);
            }
        }

        private DateTime _lastcheck;
        public DateTime LastCheck {
            get => _lastcheck;
            set {
                _lastcheck = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrLastCheck, value);
            }
        }

        public Img(
            int id,
            string name,
            string hash,
            PHashEx phashex,
            int year,
            SortedList<int, int> history,
            int bestid,
            int bestdistance,
            DateTime lastview,
            DateTime lastcheck
            )
        {
            Id = id;
            Name = name;
            Hash = hash;
            PHashEx = phashex;
            _year = year;
            History = history;
            _bestid = bestid;
            _bestdistance = bestdistance;
            _lastview = lastview;
            _lastcheck = lastcheck;
        }
    }
}