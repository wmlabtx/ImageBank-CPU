using System;
using System.Collections.Generic;

namespace ImageBank
{
    public class ColorCube
    {
        public List<byte[]> V = new List<byte[]>();

        /*
        public void Add(Vec3b vec)
        {
            var e = new byte[3];
            e[0] = vec.Item0;
            e[1] = vec.Item1;
            e[2] = vec.Item2;
            V.Add(e);
            for (var i = 0; i < 3; i++)
            {
                Min[i] = Math.Min(Min[i], e[i]);
                Max[i] = Math.Max(Max[i], e[i]);
                Sum[i] += e[i];
            }
        }
        */

        public void Add(byte[] vec)
        {
            var e = vec;
            V.Add(e);
            for (var i = 0; i < 3; i++)
            {
                Min[i] = Math.Min(Min[i], e[i]);
                Max[i] = Math.Max(Max[i], e[i]);
                Sum[i] += e[i];
            }
        }

        public byte[] Min = new byte[3] { 0xFF, 0xFF, 0xFF };
        public byte[] Max = new byte[3];
        public long[] Sum = new long[3];
        public byte[] Avg = new byte[3];
        public byte[] Size = new byte[3];

        public int Cut { get; private set; }
        public int Diagonal { get; private set; }

        public void Update()
        {
            for (var i = 0; i < 3; i++)
            {
                Avg[i] = (byte)(Sum[i] / V.Count);
                Size[i] = (byte)(Max[i] - Min[i]);
            }

            Cut = -1;
            var max = 0;
            for (var i = 0; i < 3; i++)
            {
                if (Size[i] > max)
                {
                    max = Size[i];
                    Cut = i;
                }
            }

            Diagonal = (int)Math.Sqrt(Size[0] * Size[0] + Size[1] * Size[1] + Size[2] * Size[2]);
        }
    }
}
