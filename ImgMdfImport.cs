using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Import()
        {
            lock (_rwlock) {
                _rwList.Clear();
                ((IProgress<string>)AppVars.Progress).Report($"importing {AppConsts.PathHp}...");
                var directoryInfo = new DirectoryInfo(AppConsts.PathHp);
                var fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
                lock (_imglock) {
                    foreach (var e in fs) {
                        if (!_imgList.ContainsKey(e.FullName)) {
                            _rwList.Add(e);
                        }
                    }
                }

                ((IProgress<string>)AppVars.Progress).Report($"importing {AppConsts.PathRw}...");
                directoryInfo = new DirectoryInfo(AppConsts.PathRw);
                fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
                foreach (var e in fs) {
                    if (!Path.GetExtension(e.FullName).Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                        _rwList.Add(e);
                    }
                }

                _rwList = _rwList.OrderBy(e => e.Length).ToList();
            }

            ((IProgress<string>)AppVars.Progress).Report($"clean-up {AppConsts.PathHp}...");
            Helper.CleanupDirectories(AppConsts.PathHp, AppVars.Progress);
            ((IProgress<string>)AppVars.Progress).Report($"clean-up {AppConsts.PathRw}...");
            Helper.CleanupDirectories(AppConsts.PathRw, AppVars.Progress);
        }
    }
}
