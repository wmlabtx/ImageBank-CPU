namespace ImageBank
{
    public static class AppConsts
    {
        public const string PathRw = @"D:\Users\Murad\Documents\Sdb\Rw";
        public const string FileDatabase = @"D:\Users\Murad\Documents\Sdb\Db\images.mdf";
        public const string PathGb = @"D:\Users\Murad\Documents\Sdb\Gb";
        public const string PathHp = @"D:\Users\Murad\Documents\Sdb\Hp";
        public const string PathLe = @"D:\Users\Murad\Documents\Sdb\Le";
        public const string FileSiftNodes = @"D:\Users\Murad\Documents\Sdb\Db\siftnodes.dat";

        public const string MzxExtension = ".mzx";
        public const string DatExtension = ".dat";
        public const string JpgExtension = ".jpg";
        public const string CorruptedExtension = ".corrupted";
        public const string BakExtension = ".bak";

        public const int SiftMaxNodes = 256;
        public const int SiftSplit = 1000;
        public const float SiftLimit = 500f;

        public const double WindowMargin = 5.0;
        public const double TimeLapse = 500.0;

        public const string TableImages = "Images";
        public const string AttrId = "Id";
        public const string AttrName = "Name";
        public const string AttrHash = "Hash";
        public const string AttrPHashEx = "PHashEx";
        public const string AttrVector = "Vector";
        public const string AttrYear = "Year";
        public const string AttrCounter = "Counter";
        public const string AttrBestId = "BestId";
        public const string AttrBestPDistance = "BestPDistance";
        public const string AttrBestVDistance = "BestVDistance";
        public const string AttrLastView = "LastView";
        public const string AttrLastCheck = "LastCheck";
        public const string TableVars = "Vars";
        public const string AttrImportLimit = "ImportLimit";
        public const string TableNodes = "Nodes";
        public const string AttrCore = "Core";
        public const string AttrSumDst = "SumDst";
        public const string AttrMaxDst = "MaxDst";
        public const string AttrCnt = "Cnt";
        public const string AttrAvgDst = "AvgDst";
        public const string AttrChildId = "ChildId";
    }
}