using System;
using System.Threading;

namespace ImageBank
{
    public static class AppVars
    {
        public static readonly ImgMdf Collection = new ImgMdf();
        public static readonly ImgPanel[] ImgPanel = new ImgPanel[2];
        
        public static string MoveMessage { get; set; }

        public static Progress<string> Progress { get; set; }
        public static ManualResetEvent SuspendEvent { get; set; }
    }
}