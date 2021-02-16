using System;
using System.Drawing.Imaging;
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

            foreach (var fileInfo in fileInfos) {
                var filename = fileInfo.FullName;
                var name = Path.GetFileNameWithoutExtension(filename);
                var folder = string.Empty;
                if (path.Equals(AppConsts.PathHp))
                {
                    var shortfilename = filename.Substring(AppConsts.PathHp.Length + 1);
                    folder = Path.GetDirectoryName(shortfilename);
                    if (_imgList.TryGetValue(name, out var imgfound))
                    {
                        if (!string.IsNullOrEmpty(folder) && folder.Equals(imgfound.Folder))
                        {
                            continue;
                        }

                        if (File.Exists(imgfound.FileName))
                        {
                            found++;
                            Helper.DeleteToRecycleBin(filename);
                            continue;
                        }

                        Delete(imgfound.Name);
                    }
                }

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    ((IProgress<string>)AppVars.Progress).Report($"{name} (a:{added}/b:{bad}/f:{found})...");
                }

                if (!ImageHelper.GetImageDataFromFile(
                    filename,
                    out var imagedata,
                    out var bitmap,
                    out var message)) {
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {name}: {message}");
                    bad++;
                    continue;
                }

                ImageHelper.ComputeBlob(bitmap, out var phash, out var mapdescriptors, out var descriptors);
                if (descriptors == null || descriptors.Length == 0) {
                    message = "not enough descriptors";
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {name}: {message}");
                    bad++;
                    var badname = Path.Combine(AppConsts.PathRw, $"{name}.png");
                    bitmap.Save(badname, ImageFormat.Png);
                    continue;
                }

                var blob = ImageHelper.ArrayFrom64(descriptors);

                lock (_imglock) {
                    var hash = Helper.ComputeHash(imagedata);
                    if (_hashList.TryGetValue(hash, out var imgfound)) {
                        if (File.Exists(imgfound.FileName)) {
                            found++;
                            Helper.DeleteToRecycleBin(filename);
                            continue;
                        }

                        Delete(imgfound.Name);
                    }

                    var len = 8;
                    while (len <= 32) {
                        name = hash.Substring(0, len);
                        if (!_imgList.ContainsKey(name)) {
                            break;
                        }

                        len++;
                    }
                
                    var lastadded = DateTime.Now;
                    var lastview = GetMinLastView();
                    var lastcheck = GetMinLastCheck();
                    if (path.Equals(AppConsts.PathRw)) {
                        folder = $"root\\{hash.Substring(0, 1)}";
                    }

                    var img = new Img(
                        name: name,
                        folder: folder,
                        hash: hash,
                        blob: blob,
                        mapdescriptors: mapdescriptors,
                        phash: phash,
                        lastadded: lastadded,
                        lastview: lastview,
                        counter: 0,
                        lastcheck: lastcheck,
                        nexthash: hash,
                        distance: AppConsts.MaxDistance);

                    var lastmodified = File.GetLastWriteTime(filename);
                    if (lastmodified > DateTime.Now) {
                        lastmodified = DateTime.Now;
                    }

                    if (!filename.Equals(img.FileName)) {
                        Helper.WriteData(img.FileName, imagedata);
                        File.SetLastWriteTime(img.FileName, lastmodified);
                        Helper.DeleteToRecycleBin(filename);
                    }

                    Add(img);
                    bitmap.Dispose();

                    if (_imgList.Count >= AppConsts.MaxImages) {
                        break;
                    }
                }

                added++;
                if (added >= maxadd) {
                    break;
                }
            }

            ((IProgress<string>)AppVars.Progress).Report($"clean-up...");
            Helper.CleanupDirectories(path, AppVars.Progress);

            AppVars.SuspendEvent.Set();
        }

    }
}
