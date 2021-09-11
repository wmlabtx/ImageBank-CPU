using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Import(int max, IProgress<string> progress)
        {
            lock (_rwlock) {
                _rwList.Clear();
                if (progress != null) {
                    progress.Report($"importing {AppConsts.PathHp}...");
                }

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
                                string[] names;
                                lock (_imglock) {
                                    names = _imgList.Select(img => img.Value.Name).ToArray();
                                }

                                if (Array.IndexOf(names, key) >= 0) {
                                    continue;
                                }
                            }
                        }

                        _rwList.Add(e);
                    }
                }

                if (progress != null) {
                    progress.Report($"importing {AppConsts.PathRw}...");
                }

                directoryInfo = new DirectoryInfo(AppConsts.PathRw);
                fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).OrderBy(e => e.Length).Take(max).ToArray();
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

            if (progress != null) {
                progress.Report($"clean-up {AppConsts.PathHp}...");
            }

            Helper.CleanupDirectories(AppConsts.PathHp, AppVars.Progress);

            if (progress != null) {
                progress.Report($"clean-up {AppConsts.PathRw}...");
            }

            Helper.CleanupDirectories(AppConsts.PathRw, AppVars.Progress);
        }
    }
}
