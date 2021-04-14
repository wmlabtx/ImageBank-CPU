namespace ImageBank
{
    public static class AppConsts
    {
        private const string PathRoot = @"D:\Users\Murad\Documents\Sdb\";
        public const string FileDatabase = PathRoot + @"Db\images.mdf";

        public const string PathHp = PathRoot + @"Hp";
        public const string PathRw = PathRoot + @"Rw";
        public const string FolderDefault = "root";

        public const int MaxImages = 300000;
        public const int MaxAdd = 100000;

        public const int MaxPerceptiveDistance = 64;
        public const int MinPerceptiveDistance = 8;
        public const float MaxOrbDistance = 256f;
        public const float MinOrbDistance = 16f;

        public const string MzxExtension = ".mzx";
        public const string DatExtension = ".dat";
        public const string DbxExtension = ".dbx";
        public const string WebpExtension = ".webp";
        public const string JpgExtension = ".jpg";
        public const string JpegExtension = ".jpeg";
        public const string PngExtension = ".png";
        public const string BmpExtension = ".bmp";
        public const string CorruptedExtension = ".corrupted";

        public const double WindowMargin = 5.0;
        public const double TimeLapse = 500.0;

        public const string TableImages = "Images";
        public const string AttrId = "Id";
        public const string AttrName = "Name";
        public const string AttrFolder = "Folder";
        public const string AttrHash = "Hash";

        public const string AttrWidth = "Width";
        public const string AttrHeight = "Height";
        public const string AttrSize = "Size";

        public const string AttrColorDescriptors = "ColorDescriptors";
        public const string AttrColorDistance = "ColorDistance";
        public const string AttrPerceptiveDescriptorsBlob = "PerceptiveDescriptorsBlob";
        public const string AttrPerceptiveDistance = "PerceptiveDistance";
        public const string AttrOrbDescriptorsBlob = "OrbDescriptorsBlob";
        public const string AttrOrbKeyPointsBlob = "OrbKeyPointsBlob";
        public const string AttrOrbDistance = "OrbDistance";

        public const string AttrLastChanged = "LastChanged";
        public const string AttrLastView = "LastView";
        public const string AttrLastCheck = "LastCheck";
        public const string AttrNextHash = "NextHash";
        
        public const string AttrCounter = "Counter";
        public const string AttrLastId = "LastId";
        
        public const string TableVars = "Vars";
    }
}
