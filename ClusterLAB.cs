namespace ImageBank
{
    public class ClusterLAB
    {
        public double L { get; }
        public double A { get; }
        public double B { get; }
        public int V { get; }
        public double D { get; set; }

        public ClusterLAB(double l, double a, double b, int v)
        {
            L = l;
            A = a;
            B = b;
            V = v;
            D = 0.0;
        }
    }
}
