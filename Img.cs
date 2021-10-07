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

        private MHash _mhash;
        public MHash MHash {
            get => _mhash;
            set {
                _mhash = value;
                var array = _mhash.ToArray();
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrMHash, array);
            }
        }

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

        private int _bestpdistance;
        public int BestPDistance {
            get => _bestpdistance;
            set {
                _bestpdistance = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrBestPDistance, value);
            }
        }

        private float _bestmdistance;
        public float BestMDistance {
            get => _bestmdistance;
            set {
                _bestmdistance = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrBestMDistance, value);
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
            MHash mhash,
            int year,
            SortedList<int, int> history,
            int bestid,
            int bestpdistance,
            float bestmdistance,
            DateTime lastview,
            DateTime lastcheck
            )
        {
            Id = id;
            Name = name;
            Hash = hash;
            PHashEx = phashex;
            _mhash = mhash;
            _year = year;
            History = history;
            _bestid = bestid;
            _bestpdistance = bestpdistance;
            _bestmdistance = bestmdistance;
            _lastview = lastview;
            _lastcheck = lastcheck;
        }
    }
}