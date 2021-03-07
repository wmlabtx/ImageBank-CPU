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
            var moved = 0;
            var dt = DateTime.Now;
            var folder = 0;

            ((IProgress<string>)AppVars.Progress).Report("importing...");
            var directoryInfo = new DirectoryInfo(path);
            var fileInfos = 
                directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                    .OrderBy(e => e.Length)
                    .ToArray();

            foreach (var fileInfo in fileInfos) {
                var filename = fileInfo.FullName;
                var name = Path.GetFileNameWithoutExtension(filename);
                if (path.Equals(AppConsts.PathHp))
                {
                    var shortfilename = filename.Substring(AppConsts.PathHp.Length + 1);
                    var pfolder = Path.GetDirectoryName(shortfilename);
                    if (!int.TryParse(pfolder, out folder)) {
                        folder = 1;
                    }

                    if (_imgList.TryGetValue(name, out var imgfound)) {
                        if (folder == imgfound.Folder) {
                            continue;
                        }

                        if (File.Exists(imgfound.FileName)) {
                            found++;
                            Helper.DeleteToRecycleBin(filename);
                            continue;
                        }

                        Delete(imgfound.Name);
                    }
                }

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    ((IProgress<string>)AppVars.Progress).Report($"{name} (a:{added}/b:{bad}/f:{found}/m:{moved})...");
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

                lock (_imglock) {
                    var lastadded = DateTime.Now;
                    var lastview = GetMinLastView();
                    var lastcheck = GetMinLastCheck();
                    var hash = Helper.ComputeHash(imagedata);
                    if (path.Equals(AppConsts.PathRw)) {
                        folder = 1;
                        if (_imgList.Count > 0) {
                            folder = _imgList.Max(e => e.Value.Folder);
                            var nfolder = _imgList.Count(e => e.Value.Folder == folder);
                            if (nfolder >= AppConsts.MaxImagesInFolder) {
                                folder++;
                            }
                        }
                    }
                    else {
                        var pfolder = Path.GetDirectoryName(filename).Substring(path.Length + 1);
                        if (!int.TryParse(pfolder, out folder)) {
                            folder = 1;
                        }
                    }

                    if (_hashList.TryGetValue(hash, out var imgfound)) {
                        lastadded = imgfound.LastAdded;
                        lastview = imgfound.LastView;
                        lastcheck = imgfound.LastCheck;
                        if (File.Exists(imgfound.FileName)) {
                            found++;
                            Helper.DeleteToRecycleBin(filename);
                        }
                        else {
                            var imgreplace = new Img(
                                name: imgfound.Name,
                                folder: folder,
                                hash: imgfound.Hash,
                                blob: imgfound.Blob,
                                phash: imgfound.Phash,
                                lastadded: lastadded,
                                lastview: lastview,
                                history: imgfound.History,
                                lastcheck: lastcheck,
                                nexthash: imgfound.NextHash,
                                distance: imgfound.Distance,
                                width: imgfound.Width,
                                height: imgfound.Height,
                                size: imgfound.Size);

                            var lastmodifiedfound = File.GetLastWriteTime(imgfound.FileName);
                            Helper.WriteData(imgreplace.FileName, imagedata);
                            File.SetLastWriteTime(imgreplace.FileName, lastmodifiedfound);
                            Helper.DeleteToRecycleBin(filename);

                            Delete(imgfound.Name);
                            Add(imgreplace);
                            bitmap.Dispose();

                            moved++;
                            Delete(imgfound.Name);
                        }

                        continue;
                    }

                    ImageHelper.ComputeBlob(bitmap, out var phash, out var descriptors);
                    if (descriptors == null || descriptors.Length == 0) {
                        message = "not enough descriptors";
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {name}: {message}");
                        bad++;
                        var badname = Path.Combine(AppConsts.PathRw, $"{name}.png");
                        bitmap.Save(badname, ImageFormat.Png);
                        continue;
                    }

                    var blob = ImageHelper.ArrayFrom64(descriptors);

                    var len = 8;
                    while (len <= 32) {
                        name = hash.Substring(0, len);
                        if (!_imgList.ContainsKey(name)) {
                            break;
                        }

                        len++;
                    }
                
                    var img = new Img(
                        name: name,
                        folder: folder,
                        hash: hash,
                        blob: blob,
                        phash: phash,
                        lastadded: lastadded,
                        lastview: lastview,
                        history: string.Empty,
                        lastcheck: lastcheck,
                        nexthash: hash,
                        distance: AppConsts.MaxDistance,
                        width: bitmap.Width,
                        height: bitmap.Height,
                        size: imagedata.Length);

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
