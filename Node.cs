namespace ImageBank
{
    public class Node
    {
        public float[] Descriptor { get; set; }
        public int Weight { get; set; }

        public Node(
            float[] descriptor,
            int weight
            )
        {
            Descriptor = descriptor;
            Weight = weight;
        }
    }
}
