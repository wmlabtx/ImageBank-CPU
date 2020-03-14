namespace ImageBank
{
    public static class AppConsts
    {
        private const string PathRoot = @"D:\Users\Murad\Documents\SDb\";
        public const string PathCollection = PathRoot + @"hp\";
        public const string FileDatabase = PathRoot + @"db\images.mdf";
        public const string Prefix = "mzx.";

        public const int MaxImages = 200000;
        public const int MaxImport = 200000;
        public const int MaxDescriptorsInImage = 32;

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
        public const string AttrId = "Id"; // 1 or 567 or 128092
        public const string AttrChecksum = "Checksum"; // f0s...44j (50 lenght)
        public const string AttrFamily = "Family"; // 2 or 14 or 509
        public const string AttrLastView = "LastView";
        public const string AttrNextId = "NextId"; // 1 or 567 or 128092
        public const string AttrDistance = "Distance"; // 0.0123 or 0.3439 or 
        public const string AttrLastId = "LastId"; // 1 or 567 or 128092
        public const string AttrVector = "Vector";
        public const string AttrHistory = "History";
        public const string TableVars = "Vars";
    }
}
