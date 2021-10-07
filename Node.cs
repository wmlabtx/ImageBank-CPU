using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageBank
{
    public class Node
    {
        public List<ColorLAB> Colors { get; }
        public ColorLAB Centroid { get; private set; }
        public float Size { get; private set; }

        public Node()
        {
            Colors = new List<ColorLAB>();
        }

        public void Update()
        {
            var lmin = Colors.Min(e => e.L);
            var amin = Colors.Min(e => e.A);
            var bmin = Colors.Min(e => e.B);
            var cmin = new ColorLAB(lmin, amin, bmin);

            var lmax = Colors.Max(e => e.L);
            var amax = Colors.Max(e => e.A);
            var bmax = Colors.Max(e => e.B);
            var cmax = new ColorLAB(lmax, amax, bmax);

            Size = cmin.CIEDE2000(cmax);

            var lmean = Colors.Average(e => e.L);
            var amean = Colors.Average(e => e.A);
            var bmean = Colors.Average(e => e.B);
            Centroid = new ColorLAB(lmean, amean, bmean);
        }

        public void Split(out Node a, out Node b)
        {
            var minl = Colors.Min(e => e.L);
            var index = Colors.FindIndex(e => Math.Abs(e.L - minl) < 0.0001);
            var distances = new float[Colors.Count];
            for (var i = 0; i < Colors.Count; i++) {
                distances[i] = Colors[index].CIEDE2000(Colors[i]);
            }

            var meandistance = distances.Average();
            a = new Node();
            b = new Node();
            for (var i = 0; i < Colors.Count; i++) {
                if (distances[i] < meandistance) {
                    a.Colors.Add(Colors[i]);
                }
                else {
                    b.Colors.Add(Colors[i]);
                }
            }

            a.Update();
            b.Update();
        }
    }
}
