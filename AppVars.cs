using System;

namespace ImageBank
{
    public static class AppVars
    {
        public static Progress<string> Progress { get; set; }

        private static readonly RandomMersenne _random;
        public static int IRandom(int min, int max)
        {
            var result = _random.IRandom(min, max);
            return result;
        }

        static AppVars()
        {
            var seed = Guid.NewGuid().GetHashCode();
            _random = new RandomMersenne((uint)seed);
        }
    }
}