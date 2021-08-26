﻿namespace ImageBank
{
    public static class AppConsts
    {
        public const string PathRw = @"D:\Users\Murad\Documents\Sdb\Rw";
        public const string FileDatabase = @"D:\Users\Murad\Documents\Sdb\Db\images.mdf";
        public const string PathGb = @"D:\Users\Murad\Documents\Sdb\Gb";
        public const string PathHp = @"D:\Users\Murad\Documents\Sdb\Hp";

        public const int MaxImages = 300000;
        public const int MaxGeneration = 4;
        public const int MaxCandidates = 3;
        public const int MaxDescriptors = 5000;

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
        public const string AttrColorMoments = "ColorMoments";
        public const string AttrWidth = "Width";
        public const string AttrHeight = "Height";
        public const string AttrSize = "Size";
        public const string AttrDateTaken = "DateTaken";
        public const string AttrMetadata = "Metadata";
        public const string AttrNextHash = "NextHash";
        public const string AttrSim = "Sim";
        public const string AttrLastChanged = "LastChanged";
        public const string AttrLastCheck = "LastCheck";
        public const string AttrLastView = "LastView";
        public const string AttrGeneration = "Generation";
    }
}