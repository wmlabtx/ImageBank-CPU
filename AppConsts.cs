namespace ImageBank
{
    public static class AppConsts
    {
        //private const string PathRoot = @"D:\Users\Murad\Documents\SDb\";
        private const string PathRoot = @"M:\SDb\";
        public const string PathCollection = PathRoot + @"Hp\";
        public const string FileDatabase = PathRoot + @"Db\images.mdf";
        public const string FolderLegacy = @"Legacy\";
        public const string PathLegacy = PathRoot + FolderLegacy;

        public const int MaxImages = 200000;

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
        public const string AttrId = "Id"; // f0s...44j (16 lenght)
        public const string AttrFolder = "Folder";
        public const string AttrLastView = "LastView";
        public const string AttrNextId = "NextId"; // f0s...44j (16 lenght)
        public const string AttrDistance = "Distance"; // 0.0123 or 45.3439 or 63.99
        public const string AttrLastCheck = "LastCheck";
        public const string AttrLastModified = "LastModified";
        public const string AttrVector = "Vector";
        public const string AttrCounter = "Counter";
    }
}
