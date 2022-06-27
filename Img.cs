using System;
using System.Linq;
using System.Text;

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

        public int SceneId { get; private set; }

        public void SetSceneId(int sceneid)
        {
            SceneId = sceneid;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeSceneId, SceneId);
        }

        public float Distance { get; private set; }

        public void SetDistance(float distance)
        {
            Distance = distance;
            ImgMdf.SqlImagesUpdateProperty(Id, AppConsts.AttributeDistance, Distance);
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
            int sceneid
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
            SceneId = sceneid;
        }
    }
}