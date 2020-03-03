using System;
using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public int Id { get; }
        public string Name { get; }
        public DateTime LastView { get; }
        public float Distance { get; }
        public int Generation { get; }
        public DateTime LastChange { get; }
        public Bitmap Bitmap { get; }
        public long Length { get; }

        public ImgPanel(int id, string name, DateTime lastview, int generation, float distance, DateTime lastchange, Bitmap bitmap, long length)
        {
            Id = id;
            Name = name;
            LastView = lastview;
            Generation = generation;
            Distance = distance;
            LastChange = lastchange;
            Bitmap = bitmap;
            Length = length;
        }
    }
}
