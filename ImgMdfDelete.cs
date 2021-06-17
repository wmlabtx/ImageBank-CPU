using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Delete(string filename)
        {
            lock (_imglock) {
                if (_imgList.TryGetValue(filename, out var img)) {
                    if (_hashList.ContainsKey(img.Hash)) {
                        _hashList.Remove(img.Hash);
                    }

                    foreach (var e in _imgList) {
                        if (e.Value.NextHash.Equals(img.Hash)) {
                            e.Value.NextHash = e.Value.Hash;
                            e.Value.KazeMatch = 0;
                            e.Value.LastCheck = GetMinLastCheck();
                        }
                    }

                    _imgList.Remove(filename);

                    if (File.Exists(filename)) {
                        if (filename.StartsWith(AppConsts.PathMz, StringComparison.OrdinalIgnoreCase)) {
                            var fileattributes = File.GetAttributes(filename);
                            if (!fileattributes.HasFlag(FileAttributes.Hidden)) {
                                fileattributes |= FileAttributes.Hidden;
                                File.SetAttributes(filename, fileattributes);
                            }
                        }
                        else {
                            Helper.DeleteToRecycleBin(img.FileName);
                        }
                    }
                }
            }

            SqlDelete(filename);
        }

        public static void DeleteFolder(IProgress<string> progress)
        {
            var filename = AppVars.ImgPanel[0].Img.FileName;
            if (!filename.StartsWith(AppConsts.PathMz, StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var directory = Path.GetDirectoryName(filename);
            lock (_imglock) {
                var scope = _imgList.Select(e => e.Value).ToArray();
                foreach (var e in scope) {
                    var ed = Path.GetDirectoryName(e.FileName);
                    if (ed.Equals(directory, StringComparison.OrdinalIgnoreCase)) {
                        progress.Report($"Deleting {e.FileName}... ");
                        Delete(e.FileName);
                    }
                }

                progress.Report($"Deleteing {directory}...");
                var directoryinfo = new DirectoryInfo(directory);
                var fileinfos =
                    directoryinfo.GetFiles("*.*", SearchOption.AllDirectories)
                        .OrderBy(e => e.Length)
                        .ToArray();

                foreach (var fileinfo in fileinfos) {
                    var orgfilename = fileinfo.FullName;
                    progress.Report($"Deleting {orgfilename}... ");
                    Helper.DeleteToRecycleBin(orgfilename);
                }

                Directory.Delete(directory);

                /*
                progress.Report($"Labeling {directory} as deleted...");
                var directoryinfo = new DirectoryInfo(directory);
                var fileinfos =
                    directoryinfo.GetFiles("*.*", SearchOption.AllDirectories)
                        .OrderBy(e => e.Length)
                        .ToArray();

                foreach (var fileinfo in fileinfos) {
                    var orgfilename = fileinfo.FullName;
                    var fileattributes = File.GetAttributes(orgfilename);
                    if (!fileattributes.HasFlag(FileAttributes.Hidden)) {
                        fileattributes |= FileAttributes.Hidden;
                        File.SetAttributes(orgfilename, fileattributes);
                    }
                }
                */
            }
        }
    }
}