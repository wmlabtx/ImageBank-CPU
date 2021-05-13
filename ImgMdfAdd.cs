namespace ImageBank
{
    public partial class ImgMdf
    {
        private static void AddToMemory(Img img)
        {
            lock (_imglock) { 
                _hashList.Add(img.Hash, img);
                _imgList.Add(img.Id, img);
            }
        }

        private void Add(Img img)
        {
            AddToMemory(img);
            SqlAdd(img);
        }

        private static void AddResultToMemory(int idx, int idy, int ac)
        {
            if (_resultList.TryGetValue(idx, out var nx)) {
                if (nx.ContainsKey(idy)) {
                    nx[idy] = ac;
                }
                else {
                    nx.Add(idy, ac);
                }
            }
            else {
                nx = new System.Collections.Generic.SortedDictionary<int, int> {
                    { idy, ac }
                };

                _resultList.Add(idx, nx);
            }
        }

        private static void AddResult(int idx, int idy, int ac)
        {
            if (_resultList.TryGetValue(idx, out var nx)) {
                if (nx.ContainsKey(idy)) {
                    nx[idy] = ac;
                    SqlUpdateResult(idx, idy, ac);
                }
                else {
                    nx.Add(idy, ac);
                    SqlAddResult(idx, idy, ac);
                }
            }
            else {
                nx = new System.Collections.Generic.SortedDictionary<int, int> {
                    { idy, ac }
                };

                _resultList.Add(idx, nx);
                SqlAddResult(idx, idy, ac);
            }
        }
    }
}
