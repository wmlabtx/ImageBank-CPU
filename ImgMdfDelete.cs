namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Delete(string name)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(name, out var img)) {
                    if (_hashList.ContainsKey(img.Hash)) {
                        _hashList.Remove(img.Hash);
                    }

                    foreach (var e in _nodeList) {
                        if (e.Value.Members.ContainsKey(name)) {
                            e.Value.RemoveMember(name);
                            SqlUpdateNode(e.Key, e.Value);
                        }
                    }

                    _imgList.Remove(name);

                    var filename = Helper.GetFileName(img.Name);
                    Helper.DeleteToRecycleBin(filename);
                }
            }

            SqlDelete(name);
        }
    }
}