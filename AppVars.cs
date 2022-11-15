using System;

namespace ImageBank
{
    public static class AppVars
    {
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

        private static int _familyid;
        public static int GetFamilyId()
        {
            return _familyid;
        }

        public static void SetFamilyId(int familyid)
        {
            _familyid = familyid;
        }

        public static int AllocateFamilyId()
        {
            _familyid++;

            AppDatabase.VarsUpdateProperty(AppConsts.AttributeFamilyId, _familyid);
            return _familyid;
        }

        private static readonly RandomMersenne _random;
        public static int IRandom(int min, int max)
        {
            var result = _random.IRandom(min, max);
            return result;
        }
        
        public static void SetVars(int id, int familyid)
        {
            _id = id;
            _familyid = familyid;
        }

        static AppVars()
        {
            _id = 0;
            _familyid = 0;
            var seed = Guid.NewGuid().GetHashCode();
            _random = new RandomMersenne((uint)seed);
        }
    }
}