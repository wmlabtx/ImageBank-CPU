using System;

namespace ImageBank
{
    public class Img
    {
        public int Id { get; }
        public string Name { get; }
        public string Hash { get; }

        public int Year { get; private set; }

        public void SetActualYear()
        {
            Year = DateTime.Now.Year;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeYear, Year);
        }

        public DateTime LastView { get; private set; }

        public void SetLastView(DateTime lastview)
        {
            LastView = lastview;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeLastView, LastView);
        }

        public byte[] _quantvector;

        public byte[] GetQuantVector()
        {
            return _quantvector;
        }

        public void SetQuantVector(byte[] quantvector)
        {
            _quantvector = quantvector;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeQuantVector, quantvector);
        }

        public int FamilyId { get; private set; }

        public void SetFamilyId(int familyid)
        {
            FamilyId = familyid;
            AppDatabase.ImageUpdateProperty(Id, AppConsts.AttributeFamilyId, FamilyId);
        }

        public Img(
            int id,
            string name,
            string hash,
            int year,
            byte[] quantvector,
            int familyid,
            DateTime lastview
            )
        {
            Id = id;
            Name = name;
            Hash = hash;
            Year = year;
            _quantvector = quantvector;
            FamilyId = familyid;
            LastView = lastview;
        }
    }
}