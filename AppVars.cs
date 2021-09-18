using System;
using System.Collections.Generic;
using System.Threading;

namespace ImageBank
{
    public static class AppVars
    {
        public static readonly ImgMdf Collection = new ImgMdf();
        public static readonly ImgPanel[] ImgPanel = new ImgPanel[2];
        public static readonly List<Img> Candidates = new List<Img>();
        public static int CandidateIndex;

        public static Progress<string> Progress { get; set; }
        public static ManualResetEvent SuspendEvent { get; set; }
    }
}