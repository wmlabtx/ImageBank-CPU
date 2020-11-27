using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace ImageBank
{
    public class ColorDescriptor :IEquatable<ColorDescriptor>
    {
        public int Red { get; }
        public int Green { get; }
        public int Blue { get; }

        public double H { get; }
        public double S { get; }
        public double V { get; }

        public byte Index { get; }
        
        public ColorDescriptor(int red, int green, int blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            ConvertToHSV(red, green, blue, out var dh, out var ds, out var dv);
            H = dh;
            S = ds;
            V = dv;
            var hq = (int)(H * 6.0 / 360.0);
            Debug.Assert(hq < 6);
            var sq = (int)(S * 2.9);
            Debug.Assert(sq < 3);
            var vq = (int)(V * 2.9);
            Debug.Assert(vq < 3);

            Index = (byte)((hq * 3 * 3) + (sq * 3) + vq);
        }

        private static void ConvertToHSV(int red, int green, int blue, out double dh, out double ds, out double dv)
        {
            var r = red / 255.0;
            var g = green / 255.0;
            var b = blue / 255.0;

            // h = [0,360], s = [0,1], v = [0,1]
            var min = Math.Min(Math.Min(r, g), b);
            var max = Math.Max(Math.Max(r, g), b);
            dv = max;
            var delta = max - min;
            if (blue == green && green == red) {
                ds = 0.0;
                dh = 0.0;
            }
            else {
                ds = delta / max;
                if (r == max)
                    dh = (g - b) / delta;
                else if (g == max)
                    dh = 2.0 + (b - r) / delta;
                else
                    dh = 4.0 + (r - g) / delta;

                dh *= 60.0;
                if (dh < 0) {
                    dh += 360.0;
                }
            }
        }

        public bool Equals(ColorDescriptor other)
        {
            Contract.Requires(other != null);
            return (Red == other.Red) && (Green == other.Green) && (Blue == other.Blue);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ColorDescriptor);
        }

        public override int GetHashCode()
        {
            int hc = 3;
            hc = unchecked(hc * 314159 + Red);
            hc = unchecked(hc * 314159 + Green);
            hc = unchecked(hc * 314159 + Blue);
            return hc;
        }
    }
}
