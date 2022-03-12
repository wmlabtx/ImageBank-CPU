using System.IO;

namespace ImageBank
{
    public class Axon
    {
        public int _idfrom;
        public int _idto;
        public int _age;

        public int IdFrom => _idfrom;
        public int IdTo => _idto;
        public int Age => _age;

        public Axon(int idfrom, int idto)
        {
            _idfrom = idfrom;
            _idto = idto;
            _age = 0;
        }

        public Axon(BinaryReader br)
        {
            _idfrom = br.ReadInt32();
            _idto = br.ReadInt32();
            _age = br.ReadInt32();
        }

        public void IncrementAge()
        {
            _age++;
        }

        public void ResetAge()
        {
            _age = 0;
        }

        public void Save(BinaryWriter bw)
        {
            bw.Write(_idfrom);
            bw.Write(_idto);
            bw.Write(_age);
        }
    }
}
