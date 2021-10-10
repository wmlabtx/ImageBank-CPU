namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void AddToMemory(Img img)
        {
            lock (_imglock) {
                _imgList.Add(img.Id, img);
                _nameList.Add(img.Name, img);
                _hashList.Add(img.Hash, img);
            }
        }

        private static void Add(Img img)
        {
            AddToMemory(img);
            SqlAdd(img);
        }

        private static void AddToMemory(SiftNode siftnode)
        {
            lock (_nodesLock) {
                _nodesList.Add(siftnode.Id, siftnode);
            }
        }

        private static void Add(SiftNode siftnode)
        {
            AddToMemory(siftnode);
            SqlAdd(siftnode);
        }
    }
}
