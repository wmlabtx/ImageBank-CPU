using System.IO;

namespace ImageBank
{
    public class Neuron
    {
        public ushort Id { get; }
        public float Error { get; set; }

        private readonly RootSiftDescriptor _vector;

        public Neuron(ushort id, RootSiftDescriptor vector)
        {
            Id = id;
            _vector = vector;
        }

        public Neuron(BinaryReader br)
        {
            Id = br.ReadUInt16();
            Error = br.ReadSingle();
            _vector = new RootSiftDescriptor(br);
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(Id);
            bw.Write(Error);
            _vector.Save(bw);
        }

        public float GetDistance(RootSiftDescriptor vector)
        {
            return _vector.GetDistance(vector);
        }

        public void MoveToward(RootSiftDescriptor destination, float k)
        {
            _vector.MoveToward(destination, k);
        }

        public Neuron Average(ushort newid, RootSiftDescriptor vector)
        {
            var rdescriptor = _vector.Average(vector);
            var result = new Neuron(newid, rdescriptor);
            return result;
        }

        public RootSiftDescriptor GetVector()
        {
            return _vector;
        }
    }
}
