using System;
using System.Drawing;

namespace ImageBank
{
    public class ImgPanel
    {
        public int Id { get; }
        public string Name { get; }
        public int Family { get; }
        public int FamilySize { get; }
        public DateTime LastView { get; }
        public float Distance { get; }
        public int Generation { get; }
        public Bitmap Bitmap { get; }
        public long Length { get; }
        public float Done { get; }

        public ImgPanel(int id, string name, int family, int familysize, DateTime lastview, float distance, int generation, Bitmap bitmap, long length, float done)
        {
            Id = id;
            Name = name;
            Family = family;
            FamilySize = familysize;
            LastView = lastview;
            Distance = distance;
            Generation = generation;
            Bitmap = bitmap;
            Length = length;
            Done = done;
        }
    }
}
