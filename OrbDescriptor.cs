using System;

namespace ImageBank
{
    public class OrbDescriptor
    {
        private static readonly Random Random = new Random();
        private static readonly object Distancelock = new object();

        private ulong[] Vector { get; }

        public OrbDescriptor(byte[] array, int offset)
        {
            Vector = new ulong[4];
            Buffer.BlockCopy(array, offset, Vector, 0, 32);
        }

        private int Distance(OrbDescriptor other)
        {
            var distance =
                Intrinsic.PopCnt(Vector[0] ^ other.Vector[0]) +
                Intrinsic.PopCnt(Vector[1] ^ other.Vector[1]) +
                Intrinsic.PopCnt(Vector[2] ^ other.Vector[2]) +
                Intrinsic.PopCnt(Vector[3] ^ other.Vector[3]);

            return distance;
        }

        public static float Distance(OrbDescriptor[] x, OrbDescriptor[] y)
        {
            float distance;
            lock (Distancelock)
            {
                var length = Math.Min(x.Length, y.Length);
                var pointers = new int[length];
                var distances = new int[length];
                for (var i = 0; i < length; i++) {
                    pointers[i] = i;
                    distances[i] = x[i].Distance(y[i]);
                }

                var movingcounter = 0;
                while (movingcounter < length * 2) {
                    movingcounter++;
                    var xs1 = Random.Next(length);
                    var yd1 = pointers[xs1];
                    var xs2 = xs1;
                    while (xs2 == xs1) {
                        xs2 = Random.Next(length);
                    }

                    var yd2 = pointers[xs2];
                    var dold = distances[xs1] + distances[xs2];
                    if (dold > 0) {
                        var d1 = x[xs1].Distance(y[yd2]);
                        var d2 = x[xs2].Distance(y[yd1]);
                        var dnew = d1 + d2;
                        if (dnew < dold) {
                            movingcounter = 0;
                            distances[xs1] = d1;
                            distances[xs2] = d2;
                            pointers[xs1] = yd2;
                            pointers[xs2] = yd1;
                        }
                    }
                }

                Array.Sort(distances);
                var top = Math.Max(1, length / 2);
                var isum = 0;
                for (var i = 0; i < top; i++) {
                    isum += distances[i];
                }

                distance = (float) isum / top;
            }

            return distance;
        }
    }
}
