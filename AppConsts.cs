namespace ImageBank
{
    public static class AppConsts
    {
        private const string PathRoot = @"D:\Users\Murad\Documents\SDb\";
        public const string PathHp = PathRoot + @"Hp\";
        public const string PathDt = PathRoot + @"Dt\";
        public const string PathRw = PathRoot + @"Rw\";
        public const string FileDatabase = PathRoot + @"Db\images.mdf";
        public const int MaxImages = 200000;
        public const int MaxImagesInFolder = 2000;
        public const int MaxDescriptorsInImage = 500;
        public const int MaxClustersInImage = 10; // 320 bytes

        public const string MzxExtension = ".mzx";
        public const string DatExtension = ".dat";
        public const string WebpExtension = ".webp";
        public const string JpgExtension = ".jpg";
        public const string JpegExtension = ".jpeg";
        public const string PngExtension = ".png";
        public const string BmpExtension = ".bmp";

        public const double WindowMargin = 5.0;
        public const double TimeLapse = 500.0;

        public const string TableVars = "Vars";
        public const string TableImages = "Images";
        public const string AttrName = "Name"; // (string 10 length)
        public const string AttrFolder = "Folder"; // 0..99
        public const string AttrHash = "Hash"; // (8 bytes)
        public const string AttrLastView = "LastView"; // datetime
        public const string AttrWidth = "Width"; // int
        public const string AttrHeigth = "Heigth"; // int
        public const string AttrSize = "Size"; // int
        public const string AttrDescriptors = "Descriptors"; // 3200 bytes
        public const string AttrNextName = "NextName"; // (string 10 length)
        public const string AttrLastCheck = "LastCheck"; // datetime
        public const string AttrHistory = "History"; // (string 10*10 length)
        public const string AttrFamily = "Family"; // (string 4 length)
        public const string AttrDistance = "Distance"; // (float 4 bytes)
    }
}
