using System;

namespace ImageBank
{
    public static class AppVars
    {
        public static readonly ImgPanel[] ImgPanel = new ImgPanel[2];
        public static Progress<string> Progress { get; set; }

        private static int _id;
        public static int GetId()
        {
            return _id;
        }

        public static void SetId(int id)
        {
            _id = id;
        }

        public static int AllocateId()
        {
            _id++;

            AppDatabase.VarsUpdateProperty(AppConsts.AttributeId, _id);
            return _id;
        }

        private static readonly RandomMersenne _random;
        public static int IRandom(int min, int max)
        {
            var result = _random.IRandom(min, max);
            return result;
        }
        
        public static void SetVars(int id)
        {
            _id = id;
        }

        static AppVars()
        {
            _id = 0;
            var seed = Guid.NewGuid().GetHashCode();
            _random = new RandomMersenne((uint)seed);
        }
    }
}