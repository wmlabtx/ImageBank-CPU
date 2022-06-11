namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Delete(int id)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(id, out var img)) {
                    _lastviewed.Add(img.GetPalette());
                    while (_lastviewed.Count > SIMMAX) {
                        _lastviewed.RemoveAt(0);
                    }

                    _nameList.Remove(img.Name);
                    _hashList.Remove(img.Hash);
                    _imgList.Remove(id);
                    var filename = FileHelper.NameToFileName(img.Name);
                    FileHelper.DeleteToRecycleBin(filename);
                }
            }

            SqlDeleteImage(id);
        }
    }
}