using System;
using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public int Id { get; }
        public string Name { get; }
        public DateTime LastView { get; }
        public float Sim { get; }
        public int Counter { get; }
        public Bitmap Bitmap { get; }
        public long Length { get; }
        public int Format { get; }

        public ImgPanel(int id, string name, DateTime lastview, float sim, int counter, Bitmap bitmap, long length, int format)
        {
            Id = id;
            Name = name;
            LastView = lastview;
            Sim = sim;
            Counter = counter;
            Bitmap = bitmap;
            Length = length;
            Format = format;
        }
    }
}
