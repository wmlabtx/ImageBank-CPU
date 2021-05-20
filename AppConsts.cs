﻿namespace ImageBank
{
    public static class AppConsts
    {
        public const string PathRoot = @"D:\Users\Murad\Documents\Sdb\";
        public const string FileDatabase = PathRoot + @"Db\images.mdf";

        public const string PathHp = PathRoot + @"Hp";
        public const string PathRw = PathRoot + @"Rw";
        public const string FolderDefault = "root";

        public const int MaxImages = 12000;
        public const int MaxAdd = 100;

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
        public const string AttrId = "Id";
        public const string AttrFolder = "Folder";
        public const string AttrHash = "Hash";

        public const string AttrWidth = "Width";
        public const string AttrHeight = "Height";
        public const string AttrSize = "Size";

        public const string AttrAkazeDescriptorsBlob = "AkazeDescriptorsBlob";
        public const string AttrAkazeMirrorDescriptorsBlob = "AkazeMirrorDescriptorsBlob";
        public const string AttrAkazeCentroid = "AkazeCentroid";
        public const string AttrAkazeMirrorCentroid = "AkazeMirrorCentroid";
        public const string AttrAkazePairs = "AkazePairs";

        public const string AttrLastChanged = "LastChanged";
        public const string AttrLastView = "LastView";
        public const string AttrLastCheck = "LastCheck";
        public const string AttrNextHash = "NextHash";
        
        public const string AttrCounter = "Counter";
        
        public const string TableVars = "Vars";
    }
}
