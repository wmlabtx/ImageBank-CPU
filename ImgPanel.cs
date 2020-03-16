using System;
using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public int Id { get; }
        public string Name { get; }
        public int Person { get; }
        public int PersonSize { get; }
        public DateTime LastView { get; }
        public float Distance { get; }
        public int Counter { get; }
        public Bitmap Bitmap { get; }
        public long Length { get; }
        public float Done { get; }
        public int Format { get; }

        public ImgPanel(int id, string name, int person, int personsize, DateTime lastview, float distance, int counter, Bitmap bitmap, long length, float done, int format)
        {
            Id = id;
            Name = name;
            Person = person;
            PersonSize = personsize;
            LastView = lastview;
            Distance = distance;
            Counter = counter;
            Bitmap = bitmap;
            Length = length;
            Done = done;
            Format = format;
        }
    }
}
