using System;
using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public string Id { get; }
        public string Folder { get; }
        public DateTime LastView { get; }
        public float Distance { get; }
        public int Counter { get; }
        public Bitmap Bitmap { get; }
        public long Length { get; }
        public int Year { get; }

        public ImgPanel(string id, string folder, DateTime lastview, float distance, int counter, Bitmap bitmap, long length, int year)
        {
            Id = id;
            Folder = folder;
            LastView = lastview;
            Distance = distance;
            Counter = counter;
            Bitmap = bitmap;
            Length = length;
            Year = year;
        }
    }
}
