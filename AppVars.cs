using System;

namespace ImageBank
{
    public static class AppVars
    {
        public static readonly ImgPanel[] ImgPanel = new ImgPanel[2];

        public static Progress<string> Progress { get; set; }
    }
}