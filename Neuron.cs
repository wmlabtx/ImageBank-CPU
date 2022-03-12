using System.IO;

namespace ImageBank
{
    public class Neuron
    {
        private readonly int _id;
        private float _error;
        private RootSiftDescriptor _vector;

        public int Id => _id;

        public float Error => _error;

        public Neuron(int id, RootSiftDescriptor vector)
        {
            _id = id;
            _vector = vector;
            _error = 0f;
        }

        public Neuron(BinaryReader br)
        {
            _id = br.ReadInt32();
            _error = br.ReadInt32();
            _vector = new RootSiftDescriptor(br);
        }

        public float GetDistance(RootSiftDescriptor vector)
        {
            return _vector.GetDistance(vector);
        }

        public void SetError(float error)
        {
            _error = error;
        }

        public void AddError(float delta)
        {
            _error += delta;
        }

        public void MoveToward(RootSiftDescriptor destination, float k)
        {
            _vector.MoveToward(destination, k);
        }

        public Neuron Average(int newid, RootSiftDescriptor vector)
        {
            var rdescriptor = _vector.Average(vector);
            var result = new Neuron(newid, rdescriptor);
            return result;
        }

        public RootSiftDescriptor GetVector()
        {
            return _vector;
        }

        public void Save(BinaryWriter bw)
        {
           bw.Write(_id);
           bw.Write(_error);
            _vector.Save(bw);
        }
    }
}
