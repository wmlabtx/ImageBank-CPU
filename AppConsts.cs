namespace ImageBank
{
    public static class AppConsts
    {
        private const string PathRoot = @"D:\Users\Murad\Documents\Sdb\";
        public const string FileDatabase = PathRoot + @"Db\images.mdf";

        public const string PathHp = PathRoot + @"Hp";
        public const string PathRw = PathRoot + @"Rw";
        
        public const int MaxImages = 200000;

        public const float MaxDistance = 256f;

        public const string FolderDefault = "root";

        public const string MzxExtension = ".mzx";
        public const string DatExtension = ".dat";
        public const string WebpExtension = ".webp";
        public const string JpgExtension = ".jpg";
        public const string JpegExtension = ".jpeg";
        public const string PngExtension = ".png";
        public const string BmpExtension = ".bmp";

        public const double WindowMargin = 5.0;
        public const double TimeLapse = 500.0;

        public const string TableImages = "Images";
        public const string AttrName = "Name";
        public const string AttrFolder = "Folder";
        public const string AttrHash = "Hash";
        public const string AttrDescriptors = "Descriptors";
        public const string AttrMapDescriptors = "MapDescriptors";
        public const string AttrPhash = "Phash";
        public const string AttrLastAdded = "LastAdded";
        public const string AttrLastView = "LastView";
        public const string AttrCounter = "Counter";
        public const string AttrLastCheck = "LastCheck";
        public const string AttrNextHash = "NextHash";
        public const string AttrDistance = "Distance";
    }
}
