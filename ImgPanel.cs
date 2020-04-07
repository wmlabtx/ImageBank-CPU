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
        public int Counter { get; }
        public Bitmap Bitmap { get; }
        public long Length { get; }
        public MagicFormat Format { get; }
        public DateTime LastAdded { get; }

        public ImgPanel(int id, string name, DateTime lastview, float distance, int counter, Bitmap bitmap, long length, MagicFormat format, DateTime lastadded)
        {
            Id = id;
            Name = name;
            LastView = lastview;
            Distance = distance;
            Counter = counter;
            Bitmap = bitmap;
            Length = length;
            Format = format;
            LastAdded = lastadded;
        }
    }
}
