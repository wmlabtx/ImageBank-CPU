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

        private float[] _palette;
        public float[] GetPalette()
        {
            return _palette;
        }

        public void SetPalette(float[] palette)
        {
            _palette = palette;
            var buffer = Helper.ArrayFromFloat(palette);
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributePalette, buffer);
        }

        public int Year { get; private set; }

        public void SetActualYear()
        {
            Year = DateTime.Now.Year;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeYear, Year);
        }

        public int BestId { get; private set; }

        public void SetBestId(int bestid)
        {
            BestId = bestid;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeBestId, BestId);
        }

        public DateTime LastView { get; private set; }

        public void SetLastView(DateTime lastview)
        {
            LastView = lastview;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeLastView, LastView);
        }

        public float Distance { get; private set; }

        public void SetDistance(float distance)
        {
            Distance = distance;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeDistance, Distance);
        }

        private readonly int[] _ni;
        public int[] GetNi()
        {
            return _ni;
        }

        private readonly byte[] _nr;
        public byte[] GetNr()
        {
            return _nr;
        }

        public int GetNexts()
        {
            var count = _ni.Count(e => e != 0);
            return count;
        }

        public int GetRating()
        {
            var count = _nr.Count(e => e != 0);
            return count;
        }

        public void AddRank(int next, byte rank)
        {
            for (var i = 0; i < _ni.Length; i++) {
                if (_ni[i] == 0) {
                    _ni[i] = next;
                    ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeNi, Helper.ArrayFrom32(_ni));
                    _nr[i] = rank;
                    ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeNr, _nr);
                    break;
                }
            }
        }

        public void RemoveRank(int next)
        {
            for (var i = 0; i < _ni.Length; i++) {
                if (_ni[i] == next) {
                    _ni[i] = 0;
                    ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeNi, Helper.ArrayFrom32(_ni));
                    break;
                }
            }
        }

        public bool IsRank(int next)
        {
            for (var i = 0; i < _ni.Length; i++) {
                if (_ni[i] == next) {
                    return true;
                }
            }

            return false;
        }

        public Img(
            int id,
            string name,
            string hash,
            float[] palette,
            float distance,
            int year,
            int bestid,
            DateTime lastview,
            int[] ni,
            byte[] nr
        )
        {
            Id = id;
            Name = name;
            Hash = hash;
            _palette = palette;
            Year = year;
            BestId = bestid;
            Distance = distance;
            LastView = lastview;
            _ni = ni;
            _nr = nr;
        }
    }
}