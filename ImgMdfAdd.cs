namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void AddToMemory(Img img)
        {
            lock (_imglock) {
                _imgList.Add(img.Name, img);
                _hashList.Add(img.Hash, img);
            }
        }

        private static void AddToMemory(Node node)
        {
            lock (_imglock) {
                _nodeList.Add(node.NodeId, node);
            }
        }

        private static void Add(Img img)
        {
            AddToMemory(img);
            SqlAdd(img);
        }

        private static void Add(Node node)
        {
            AddToMemory(node);
            SqlAdd(node);
        }
    }
}
