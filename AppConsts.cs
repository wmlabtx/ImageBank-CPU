namespace ImageBank
{
    public static class AppConsts
    {
        public const string PathRw = @"D:\Users\Murad\Documents\Sdb\Rw";
        public const string FileDatabase = @"D:\Users\Murad\Documents\Sdb\Db\images.mdf";
        public const string PathGb = @"D:\Users\Murad\Documents\Sdb\Gb";
        public const string PathHp = @"D:\Users\Murad\Documents\Sdb\Hp";
        public const string PathLe = @"D:\Users\Murad\Documents\Sdb\Le";

        public const string MzxExtension = ".mzx";
        public const string DatExtension = ".dat";
        public const string JpgExtension = ".jpg";
        public const string CorruptedExtension = ".corrupted";
        public const string BakExtension = ".bak";

        public const int MaxClusters = 32000;
        public const int MaxDescriptors = 500;
        public const float MaxDistance = 400f;

        public const double WindowMargin = 5.0;
        public const double TimeLapse = 500.0;

        public const string TableImages = "Images";
        public const string AttrId = "Id";
        public const string AttrName = "Name";
        public const string AttrHash = "Hash";
        public const string AttrVector = "Vector";
        public const string AttrYear = "Year";
        public const string AttrCounter = "Counter";
        public const string AttrBestId = "BestId";
        public const string AttrBestVDistance = "BestVDistance";
        public const string AttrLastView = "LastView";
        public const string AttrLastCheck = "LastCheck";
        public const string AttrSig = "Sig";
        public const string TableVars = "Vars";
        public const string AttrImportLimit = "ImportLimit";
        public const string TableClusters = "Clusters";
        public const string AttrDescriptor = "Descriptor";
    }
}