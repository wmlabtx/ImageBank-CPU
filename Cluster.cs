namespace ImageBank
{
    public class Cluster
    {
        public int Id { get; }

        public int Counter { get; private set; }
        public void SetCounter(int counter)
        {
            Counter = counter;
            AppDatabase.ClusterUpdateProperty(Id, AppConsts.AttributeCounter, Counter);
        }

        public int Age { get; private set; }
        public void SetAge(int age)
        {
            Age = age;
            AppDatabase.ClusterUpdateProperty(Id, AppConsts.AttributeAge, Age);
        }

        private float[] _vector;
        public float[] GetVector()
        {
            return _vector;
        }

        public void SetVector(float[] vector)
        {
            _vector = vector;
            AppDatabase.ClusterUpdateProperty(Id, AppConsts.AttributeVector, Helper.ArrayFromFloat(_vector));
        }

        public Cluster(int id, int counter, int age, float[] vector)
        {
            Id = id;
            Counter = counter;
            Age = age;
            _vector = vector;
        }
    }
}
