using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }

        public string Checksum { get; }

        public string Name => Helper.GetName(Id);

        public string Folder => Helper.GetFolder(Id);

        public string FileName => Helper.GetFileName(Name, Folder);

        private int _family;
        public int Family
        {
            get => _family;
            set
            {
                _family = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrFamily, value);
            }
        }

        private int _nextid;
        public int NextId
        {
            get => _nextid;
            set
            {
                _nextid = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrNextId, value);
            }
        }

        private float _distance;
        public float Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrDistance, value);
            }
        }

        private int _lastid;
        public int LastId
        {
            get => _lastid;
            set
            {
                _lastid = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastId, value);
            }
        }

        private DateTime _lastview;
        public DateTime LastView
        {
            get => _lastview;
            set
            {
                _lastview = value;
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrLastView, value);
            }
        }

        private readonly Mat _vector;

        public Mat Vector()
        {
            return _vector;
        }

        private readonly SortedDictionary<int, object> _history = new SortedDictionary<int, object>();
        
        private byte[] _historyencoded;

        public byte[] GetHistoryEncoded()
        {
            return _historyencoded;
        }

        public void SetHistoryEncoded(byte[] value)
        {
            Contract.Requires(value != null);
            if (_historyencoded == null || !value.SequenceEqual(_historyencoded)) {
                _history.Clear();
                var offset = 0;
                while (offset < value.Length) {
                    var id = BitConverter.ToInt32(value, offset);
                    offset += sizeof(int);
                    _history.Add(id, null);
                }

                _historyencoded = value;
            }
        }

        public int[] GetHistoryIds()
        {
            return _history.Select(e => e.Key).ToArray();
        }

        private void EncodeHistory()
        {
            var ids = GetHistoryIds();
            var lenght = ids.Length * sizeof(int);
            _historyencoded = new byte[lenght];
            Buffer.BlockCopy(ids, 0, _historyencoded, 0, _historyencoded.Length);
        }

        public void AddToHistory(int id)
        {
            if (!_history.ContainsKey(id)) {
                _history.Add(id, null);
                EncodeHistory();
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrHistory, _historyencoded);
            }
        }

        public void RemoveFromHistory(int id)
        {
            if (_history.ContainsKey(id)) {
                _history.Remove(id);
                EncodeHistory();
                ImgMdf.SqlUpdateProperty(Id, AppConsts.AttrHistory, _historyencoded);
            }
        }

        public int Generation => _history.Count;

        public Img(
            int id,
            string checksum,
            int family,
            DateTime lastview,
            int nextid,
            float distance,
            int lastid,
            Mat vector,
            byte[] history)
        {
            Id = id;
            Checksum = checksum;
            _family = family;
            _lastview = lastview;
            _nextid = nextid;
            _distance = distance;
            _lastid = lastid;
            _vector = vector;
            SetHistoryEncoded(history ?? Array.Empty<byte>());
        }
    }
}