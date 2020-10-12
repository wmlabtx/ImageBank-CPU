using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Import(string path, int maxadd)
        { 
            AppVars.SuspendEvent.Reset();

            var added = 0;
            var found = 0;
            var replace = 0;
            var bad = 0;
            var dt = DateTime.Now;

            ((IProgress<string>)AppVars.Progress).Report($"importing...");
            var directoryInfo = new DirectoryInfo(path);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            var random = new Random();
            while (fileInfos.Count > 0) {
                var rindex = random.Next(fileInfos.Count);
                var fileInfo = fileInfos[rindex];
                fileInfos.RemoveAt(rindex);

                if (added >= maxadd) {
                    break;
                }

                var filename = fileInfo.FullName;
                var shortfilename = filename.Substring(AppConsts.PathHp.Length);
                var family = string.Empty;
                var pars = shortfilename.Split('\\');
                if (pars.Length > 1) {
                    var fsb = new StringBuilder();
                    for (var i = 0; i < pars.Length - 1; i++) {
                        if (i > 0) {
                            fsb.Append('.');
                        }

                        fsb.Append(pars[i]);
                    }

                    family = fsb.ToString();
                }

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    ((IProgress<string>)AppVars.Progress).Report($"{shortfilename} (a:{added}/f:{found}/r:{replace}/b:{bad})...");
                }

                var name = Path.GetFileNameWithoutExtension(filename);
                lock (_imglock) {
                    if (_imgList.TryGetValue(name, out Img imgfound)) {
                        var subpath = Path.GetDirectoryName(filename);
                        if (subpath.StartsWith(AppConsts.PathHp, StringComparison.OrdinalIgnoreCase)) {
                            var lastpart = subpath.Substring(AppConsts.PathHp.Length);
                            if (int.TryParse(lastpart, out int ffolder)) {
                                if (imgfound.Folder == ffolder) {
                                    continue;
                                }
                            }
                        }
                    }
                }

                if (!Helper.GetImageDataFromFile(
                    filename,
                    out byte[] imagedata,
#pragma warning disable CA2000 // Dispose objects before losing scope
                    out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                    out ulong hash,
                    out string message)) {
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    continue;
                }

                if (!DescriptorHelper.Compute(bitmap, out var descriptors)) {
                    message = "not enough descriptors";
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    continue;
                }

                lock (_imglock) {
                    if (_hashList.TryGetValue(hash, out string namefound)) {
                        var imgfound = _imgList[namefound];
                        imgfound.Descriptors = descriptors;
                        imgfound.LastAdded = DateTime.Now;
                        if (!imgfound.Family.Equals(family, StringComparison.OrdinalIgnoreCase)) {
                            imgfound.Family = family;
                            var minlc = _imgList.Min(e => e.Value.LastCheck).AddSeconds(-1);
                            imgfound.NextName = "0123456789";
                            imgfound.LastCheck = minlc;
                        }

                        found++;
                        Helper.DeleteToRecycleBin(filename);
                        continue;
                    }
                }

                var folder = 0;
                lock (_imglock) {
                    do {
                        name = Helper.RandomName();
                    } while (_imgList.ContainsKey(name));

                    if (_imgList.Count > 0) {
                        folder = _imgList.Max(e => e.Value.Folder);
                        var nfolder = _imgList.Count(e => e.Value.Folder == folder);
                        if (nfolder >= AppConsts.MaxImagesInFolder) {
                            folder++;
                        }
                    } 
                }

                var imgfilename = Helper.GetFileName(name, folder);
                var lastmodified = File.GetLastWriteTime(filename);
                if (lastmodified > DateTime.Now) {
                    lastmodified = DateTime.Now;
                }

                Helper.WriteData(imgfilename, imagedata);
                File.SetLastWriteTime(imgfilename, lastmodified);
                Helper.DeleteToRecycleBin(filename);

                var lastview = GetMinLastView();
                var lastcheck = _imgList.Min(e => e.Value.LastCheck).AddSeconds(-1);
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
                    lastadded: DateTime.Now,
                    nextname: "0123456789",
                    distance: 0f,
                    family: family);

                Add(img);
                bitmap.Dispose();

                if (_imgList.Count >= AppConsts.MaxImages) {
                    break;
                }

                added++;
            }

            //((IProgress<string>)AppVars.Progress).Report($"clean-up...");
            //Helper.CleanupDirectories(path, AppVars.Progress);


            AppVars.SuspendEvent.Set();
        }
    }
}
