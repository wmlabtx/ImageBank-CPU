namespace ImageBank
{
    public static class AppConsts
    {
        public const string PathRw = @"D:\Users\Murad\Documents\Sdb\Rw";
        public const string FileDatabase = @"D:\Users\Murad\Documents\Sdb\Db\images.mdf";
        public const string FileKazeClusters = @"D:\Users\Murad\Documents\Sdb\Db\kazeclusters.dat";
        public const string PathGb = @"D:\Users\Murad\Documents\Sdb\Gb";
        public const string PathHp = @"D:\Users\Murad\Documents\Sdb\Hp";

        public const int MaxImages = 200000;
        public const int MaxImagesInGeneration = 100000;

        public const int MaxDescriptors = 1000;
        public const int DescriptorSize = 61;

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
        public const string AttrName = "Name";
        public const string AttrHash = "Hash";
        public const string AttrWidth = "Width";
        public const string AttrHeight = "Height";
        public const string AttrSize = "Size";
        public const string AttrDateTaken = "DateTaken";
        public const string AttrMetadata = "Metadata";
        public const string AttrKazeOne = "KazeOne";
        public const string AttrKazeTwo = "KazeTwo";
        public const string AttrNextHash = "NextHash";
        public const string AttrKazeMatch = "KazeMatch";
        public const string AttrLastChanged = "LastChanged";
        public const string AttrLastCheck = "LastCheck";
        public const string AttrLastView = "LastView";
        public const string AttrGeneration = "Generation";
    }
}
