namespace ImageBank
{
    public class ColorRGB
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public ColorRGB(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }
}
