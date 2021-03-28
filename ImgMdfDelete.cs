namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Delete(string name)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(name, out var img)) {
                    Helper.DeleteToRecycleBin(img.FileName);
                    _imgList.Remove(name);

                    if (_hashList.ContainsKey(img.Hash)) {
                        _hashList.Remove(img.Hash);
                    }

                    foreach (var e in _imgList) {
                        if (e.Value.NextHash.Equals(img.Hash)) {
                            e.Value.NextHash = e.Value.Hash;
                            e.Value.LastId = 0;
                            e.Value.ColorDistance = 100f;
                            e.Value.PerceptiveDistance = AppConsts.MaxPerceptiveDistance;
                            e.Value.OrbDistance = AppConsts.MaxOrbDistance;
                            e.Value.LastCheck = GetMinLastCheck();
                        }
                    }
                }
            }

            SqlDelete(name);
        }
    }
}