namespace ImageBank
{
    public static class AppConsts
    {
        public const string PathRw = @"D:\Users\Murad\Documents\Sdb\Rw";
        public const string FileDatabase = @"D:\Users\Murad\Documents\Sdb\Db\images.mdf";
        public const string PathGb = @"D:\Users\Murad\Documents\Sdb\Gb";
        public const string PathHp = @"D:\Users\Murad\Documents\Sdb\Hp";
        public const string FileWords = @"D:\Users\Murad\Documents\Sdb\Db\images_words.dat";

        public const int MaxImages = 300000;

        public const int FeatureSim = 80;
        public const int MaxNodes = 5000000;
        public const float AkazeThreshold = 0.0001f;

        public const string MzxExtension = ".mzx";
        public const string DatExtension = ".dat";
        public const string DbxExtension = ".dbx";
        public const string WebpExtension = ".webp";
        public const string JpgExtension = ".jpg";
        public const string JpegExtension = ".jpeg";
        public const string PngExtension = ".png";
        public const string BmpExtension = ".bmp";
        public const string CorruptedExtension = ".corrupted";
        public const string BakExtension = ".bak";

        public const double WindowMargin = 5.0;
        public const double TimeLapse = 500.0;

        public const string TableImages = "Images";
        public const string AttrName = "Name";
        public const string AttrHash = "Hash";
        public const string AttrDateTaken = "DateTaken";
        public const string AttrFamily = "Family";
        public const string AttrBestNames = "BestNames";
        public const string AttrLastChanged = "LastChanged";
        public const string AttrLastCheck = "LastCheck";
        public const string AttrLastView = "LastView";
        public const string AttrGeneration = "Generation";

        public const string TableNodes = "Nodes";
        public const string AttrNodeId = "NodeId";
        public const string AttrCore = "Core";
        public const string AttrDepth = "Depth";
        public const string AttrChildId = "ChildId";
        public const string AttrMembers = "Members";
        public const string AttrLastAdded = "LastAdded";
    }
}