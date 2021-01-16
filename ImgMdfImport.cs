using System;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Import(string path, int maxadd)
        { 
            AppVars.SuspendEvent.Reset();

            var added = 0;
            var bad = 0;
            var found = 0;
            var dt = DateTime.Now;

            ((IProgress<string>)AppVars.Progress).Report("importing...");
            var directoryInfo = new DirectoryInfo(path);
            var fileInfos = 
                directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                    .OrderBy(e => e.FullName)
                    .ToArray();
            foreach (var fileInfo in fileInfos)
            {
                if (added >= maxadd) {
                    break;
                }

                var filename = fileInfo.FullName;
                var shortfilename = filename.Substring(AppConsts.PathHp.Length);
                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    ((IProgress<string>)AppVars.Progress).Report($"{shortfilename} (a:{added}/b:{bad}/f:{found})...");
                }

                var name = Path.GetFileNameWithoutExtension(filename);
                lock (_imglock) {
                    if (_imgList.TryGetValue(name, out var imgfound)) {
                        var subpath = Path.GetDirectoryName(filename);
                        if (subpath != null && subpath.StartsWith(AppConsts.PathHp, StringComparison.OrdinalIgnoreCase)) {
                            var lastpart = subpath.Substring(AppConsts.PathHp.Length);
                            if (int.TryParse(lastpart, out var ffolder)) {
                                if (imgfound.Folder == ffolder) {
                                    continue;
                                }
                            }
                        }
                    }
                }

                if (!ImageHelper.GetImageDataFromFile(
                    filename,
                    out var imagedata,
                    out var bitmap,
                    out var message)) {
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    continue;
                }

                if (!ImageHelper.ComputeDescriptors(bitmap, out var descriptors)) {
                    message = "not enough descriptors";
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    continue;
                }

                lock (_imglock) {
                    var hash = Helper.ComputeHash(imagedata);
                    if (_hashList.ContainsKey(hash)) {
                        found++;
                        Helper.DeleteToRecycleBin(filename);
                        continue;
                    }

                    var len = 4;
                    while (len <= 12) {
                        name = $"mzx-{hash.Substring(0, len)}";
                        if (!_imgList.ContainsKey(name)) {
                            break;
                        }

                        len++;
                    }
                
                    var folder = 0;
                    if (_imgList.Count > 0) {
                        folder = _imgList.Max(e => e.Value.Folder);
                        var nfolder = _imgList.Count(e => e.Value.Folder == folder);
                        if (nfolder >= AppConsts.MaxImagesInFolder) {
                            folder++;
                        }
                    }

                    var imgfilename = Helper.GetFileName(name, folder);
                    var lastmodified = File.GetLastWriteTime(filename);
                    if (lastmodified > DateTime.Now) {
                        lastmodified = DateTime.Now;
                    }

                    if (!filename.Equals(imgfilename)) {
                        Helper.WriteData(imgfilename, imagedata);
                        File.SetLastWriteTime(imgfilename, lastmodified);
                        Helper.DeleteToRecycleBin(filename);
                    }

                    var lastadded = DateTime.Now;
                    var lastview = GetMinLastView();
                    var lastcheck = _imgList.Count == 0 ? 
                        DateTime.Now : 
                        _imgList.Min(e => e.Value.LastCheck).AddSeconds(-1);

                    var img = new Img(
                        name: name,
                        hash: hash,
                        width: bitmap.Width,
                        heigth: bitmap.Height,
                        size: imagedata.Length,
                        descriptors: descriptors,
                        folder: folder,
                        lastview: lastview,
                        lastcheck: lastcheck,
                        lastadded: lastadded,
                        nextname: name,
                        sim: 0f,
                        family: string.Empty,
                        counter: 0);

                    Add(img);
                    bitmap.Dispose();

                    if (_imgList.Count >= AppConsts.MaxImages) {
                        break;
                    }
                } 
                
                added++;
            }

            ((IProgress<string>)AppVars.Progress).Report($"clean-up...");
            Helper.CleanupDirectories(path, AppVars.Progress);

            AppVars.SuspendEvent.Set();
        }
    }
}
