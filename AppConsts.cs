namespace ImageBank
{
    public static class AppConsts
    {
        public const string PathRoot = @"D:\Users\Murad\Documents\Sdb";
        public const string FolderDb = @"Db";
        public const string FileVgg = PathRoot + @"\" + FolderDb +@"\" + @"resnet152-v2-7.onnx";
        public const string FileDatabase = PathRoot + @"\" + FolderDb + @"\" + "images.mdf";
        public const string FolderRw = @"Rw";
        public const string PathRw = PathRoot + @"\" + FolderRw;
        public const string FolderHp = @"Hp";
        public const string PathHp = PathRoot + @"\" + FolderHp;
        public const string FolderGb = @"Gb";
        public const string PathGb = PathRoot + @"\" + FolderGb;

        public const string PngExtension = ".png";
        public const string MzxExtension = ".mzx";
        public const string DatExtension = ".dat";
        public const string CorruptedExtension = ".corrupted";

        public const char CharEllipsis = '\u2026';
        public const char CharRightArrow = '\u2192';

        public const double WindowMargin = 5.0;
        public const double TimeLapse = 500.0;

        public const string TableImages = "Images";
        public const string AttributeName = "Name";
        public const string AttributeHash = "Hash";
        public const string AttributeCounter = "Counter";
        public const string AttributeLastView = "LastView";
        public const string AttributeLastCheck = "LastCheck";
        public const string AttributeBestHash = "BestHash";
        public const string AttributeDistance = "Distance";
        public const string AttributeVector = "Vector";
        public const string AttributeOrientation = "Orientation";
    }
}