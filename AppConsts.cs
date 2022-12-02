namespace ImageBank
{
    public static class AppConsts
    {
        public const string PathRoot = @"M:\Sdb";
        public const string FolderDb = @"Db";
        public const string FileVgg = PathRoot + @"\" + FolderDb +@"\" + @"resnet152-v2-7.onnx";
        public const string FileDatabase = PathRoot + @"\" + FolderDb + @"\" + "images.mdf";
        public const string FolderRw = @"Rw";
        public const string PathRw = PathRoot + @"\" + FolderRw;
        public const string FolderLe = @"Le";
        public const string PathLe = PathRoot + @"\" + FolderLe;
        public const string FolderGb = @"Gb";
        public const string PathGb = PathRoot + @"\" + FolderGb;

        public const string PngExtension = ".png";
        public const string MzxExtension = ".mzx";
        public const string DatExtension = ".dat";
        public const string CorruptedExtension = ".corrupted";

        public const double WindowMargin = 5.0;
        public const double TimeLapse = 500.0;

        public const string TableImages = "Images";
        public const string AttributeName = "Name";
        public const string AttributeHash = "Hash";
        public const string AttributeYear = "Year";
        public const string AttributeLastView = "LastView";
        public const string AttributeVector = "Vector";
    }
}