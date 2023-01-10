using System;
using System.Threading;

namespace ImageBank
{
    public static class AppVars
    {
        public static Progress<string> Progress { get; set; }
        public static bool ShowXOR { get; set; }

        private static readonly RandomMersenne _random;
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

        static AppVars()
        {
            var seed = Guid.NewGuid().GetHashCode();
            _random = new RandomMersenne((uint)seed);
            ShowXOR = false;
        }
    }
}