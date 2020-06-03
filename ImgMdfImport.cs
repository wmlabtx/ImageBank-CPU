using System;
using System.Drawing;
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
            var found = 0;
            var replace = 0;
            var bad = 0;
            var dt = DateTime.Now;

            ((IProgress<string>)AppVars.Progress).Report($"importing...");
            var directoryInfo = new DirectoryInfo(path);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            foreach (var fileInfo in fileInfos) {
                if (added >= maxadd) {
                    break;
                }

                var filename = fileInfo.FullName;
                var shortfilename = filename.Substring(AppConsts.PathHp.Length);

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

                lock (_imglock) {
                    if (_hashList.TryGetValue(hash, out string namefound)) {
                        var imgfound = _imgList[namefound];

                        found++;
                        Helper.DeleteToRecycleBin(filename);
                        continue;
                    }
                }

                if (!OrbHelper.Compute(bitmap, out ulong phash, out ulong[] descriptors)) {
                    message = "not enough descriptors";
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    continue;
                }

                lock (_imglock) {
                    var array = _imgList.Select(e => e.Value).ToArray();
                    foreach (var e in array) {
                        var distance = Intrinsic.PopCnt(phash ^ e.PHash);
                        if (distance <= AppConsts.MaxHamming) {
                            if (
                                (bitmap.Width == e.Width && bitmap.Height == e.Heigth) ||
                                (bitmap.Height == e.Width && bitmap.Width == e.Heigth)
                                ) {
                                if (imagedata.Length > e.Size) {
                                    replace++;
                                    Delete(e.Name);
                                }
                                else {
                                    found++;
                                    Helper.DeleteToRecycleBin(filename);
                                    continue;
                                }
                            }
                        }
                    }
                }

                var scd = new Scd(bitmap);

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

                var lastview = DateTime.Now.AddYears(-10);
                var img = new Img(
                    name: name,
                    hash: hash,
                    phash: phash,
                    width: bitmap.Width,
                    heigth: bitmap.Height,
                    size: imagedata.Length,
                    scd: scd,
                    descriptors: descriptors,
                    folder: folder,
                    path: string.Empty,
                    counter: 0,
                    lastview: lastview
                    );

                bitmap.Dispose();

                Add(img);
                if (!filename.Equals(img.FileName, StringComparison.OrdinalIgnoreCase)) {
                    var lastmodified = File.GetLastWriteTime(filename);
                    Helper.WriteData(img.FileName, imagedata);
                    File.SetLastWriteTime(img.FileName, lastmodified);
                    Helper.DeleteToRecycleBin(filename);
                }

                if (_imgList.Count >= AppConsts.MaxImages) {
                    break;
                }

                added++;
            }

            ((IProgress<string>)AppVars.Progress).Report($"clean-up...");
            Helper.CleanupDirectories(path, AppVars.Progress);

            AppVars.SuspendEvent.Set();
        }
    }
}
