using System;
using System.Threading;

namespace ImageBank
{
    public static class AppVars
    {
        public static Progress<string> Progress { get; set; }
        public static ManualResetEvent SuspendEvent { get; set; }
        public static bool ShowXOR { get; set; }
        public static bool ImportRequested { get; set; }

        private static readonly RandomMersenne _random = new RandomMersenne((uint)Guid.NewGuid().GetHashCode());
        public static int IRandom(int min, int max)
        {
            int result;
            if (Monitor.TryEnter(_random, AppConsts.LockTimeout)) {
                try {
                    result = _random.IRandom(min, max);
                }
                finally { 
                    Monitor.Exit(_random); 
                }
            }
            else {
                throw new Exception();
            }

            return result;
        }

        public static DateTime DateTakenLast { get; set; }
    }
}