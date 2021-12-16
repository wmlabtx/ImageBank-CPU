﻿using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public string Name { get; }
        public string Hash { get; }

        private int[] _vector;
        public int[] Vector {
            get => _vector;
            set {
                _vector = value;
                var array = Helper.ArrayFrom32(_vector);
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrVector, array);
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

        private int _counter;
        public int Counter {
            get => _counter;
            set {
                _counter = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrCounter, value);
            }
        }

        private int _bestid;
        public int BestId {
            get => _bestid;
            set {
                _bestid = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrBestId, value);
            }
        }

        private int _sig;
        public int Sig {
            get => _sig;
            set {
                _sig = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrSig, value);
            }
        }

        private float _bestvdistance;
        public float BestVDistance {
            get => _bestvdistance;
            set {
                _bestvdistance = value;
                ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttrBestVDistance, value);
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
            int[] vector,
            int year,
            int counter,
            int bestid,
            int sig,
            float bestvdistance,
            DateTime lastview,
            DateTime lastcheck
            )
        {
            Id = id;
            Name = name;
            Hash = hash;
            _vector = vector;
            _year = year;
            _counter = counter;
            _bestid = bestid;
            _sig = sig;
            _bestvdistance = bestvdistance;
            _lastview = lastview;
            _lastcheck = lastcheck;
        }
    }
}