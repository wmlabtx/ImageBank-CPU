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
                        var orgfilename = e.FullName;
                        var p1 = Path.GetDirectoryName(orgfilename).Substring(AppConsts.PathHp.Length + 1);
                        if (p1.Length == 2) {
                            var p2 = Path.GetFileNameWithoutExtension(orgfilename);
                            if (p2.Length == 8) {
                                var key = $"{p1}{p2}";
                                lock (_imglock) {
                                    if (_imgList.ContainsKey(key)) {
                                        continue;
                                    }
                                }
                            }
                        }

                        _rwList.Add(e);
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
                _added = 0;
                _found = 0;
                _bad = 0;
            }

            ((IProgress<string>)AppVars.Progress).Report($"clean-up {AppConsts.PathHp}...");
            Helper.CleanupDirectories(AppConsts.PathHp, AppVars.Progress);
            ((IProgress<string>)AppVars.Progress).Report($"clean-up {AppConsts.PathRw}...");
            Helper.CleanupDirectories(AppConsts.PathRw, AppVars.Progress);
        }
    }
}
