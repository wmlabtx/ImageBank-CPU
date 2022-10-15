using System;
using System.Threading;

namespace ImageBank
{
    public static class AppVars
    {
        public static readonly ImgPanel[] ImgPanel = new ImgPanel[2];
        public static Progress<string> Progress { get; set; }
        public static ManualResetEvent SuspendEvent { get; set; }
        public static bool ImportMode { get; set; }

        private static readonly object _varlock = new object();

        private static int _id;
        public static int GetId()
        {
            lock (_varlock) {
                return _id;
            }
        }

        public static void SetId(int id)
        {
            lock (_varlock) {
                _id = id;
            }

            AppDatabase.VarsUpdateProperty(AppConsts.AttributeId, id);
        }

        public static int AllocateId()
        {
            lock (_varlock) {
                _id++;
            }

            AppDatabase.VarsUpdateProperty(AppConsts.AttributeId, _id);
            return _id;
        }

        private static readonly RandomMersenne _random;
        public static int IRandom(int min, int max)
        {
            var result = _random.IRandom(min, max);
            return result;
        }
        
        private static float[] _palette;
        public static float[] GetPalette()
        {
            return _palette;
        }

        public static void SetPalette(float[] palette)
        {
            _palette = palette;
        }

        public static void SetVars(int id, float[] palette)
        {
            lock (_varlock) {
                _id = id;
                _palette = palette;
            }
        }

        static AppVars()
        {
            lock (_varlock) {
                _id = 0;
                var seed = Guid.NewGuid().GetHashCode();
                _random = new RandomMersenne((uint)seed);
                _palette = new float[256 * 3];
            }
        }
    }
}