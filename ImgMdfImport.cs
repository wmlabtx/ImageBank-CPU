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
            var moved = 0;
            var dt = DateTime.Now;
            var folder = string.Empty;

            ((IProgress<string>)AppVars.Progress).Report("importing...");
            var directoryInfo = new DirectoryInfo(path);
            var fileInfos = 
                directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                    .OrderBy(e => e.Length)
                    .ToArray();

            foreach (var fileInfo in fileInfos) {
                var filename = fileInfo.FullName;
                var name = Path.GetFileNameWithoutExtension(filename);
                var extension = Path.GetExtension(filename);
                if (extension.Equals(AppConsts.CorruptedExtension)) {
                    continue;
                }

                if (path.Equals(AppConsts.PathHp)) {
                    var shortfilename = filename.Substring(AppConsts.PathHp.Length + 1);
                    folder = Path.GetDirectoryName(shortfilename);
                    if (_imgList.TryGetValue(name, out var imgfound)) {
                        if (folder.Equals(imgfound.Folder)) {
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
                    File.Move(filename, $"{filename}{AppConsts.CorruptedExtension}");
                    continue;
                }

                lock (_imglock) {
                    var lastchanged = DateTime.Now;
                    var lastview = GetMinLastView();
                    if (
                        !extension.Equals(AppConsts.MzxExtension, StringComparison.OrdinalIgnoreCase) &&
                        !extension.Equals(AppConsts.DbxExtension, StringComparison.OrdinalIgnoreCase) &&
                        !extension.Equals(AppConsts.DatExtension, StringComparison.OrdinalIgnoreCase)
                        ){
                        lastview = _viewnow;
                    }

                    var lastcheck = GetMinLastCheck();
                    var hash = Helper.ComputeHash(imagedata);
                    if (path.Equals(AppConsts.PathRw)) {
                        folder = Helper.ComputeFolder(imagedata);
                    }

                    if (_hashList.TryGetValue(hash, out var imgfound)) {
                        lastchanged = imgfound.LastChanged;
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
                                pblob: imgfound.PBlob,
                                lastchanged: lastchanged,
                                lastview: lastview,
                                counter: imgfound.Counter,
                                lastcheck: lastcheck,
                                nexthash: imgfound.NextHash,
                                diff: imgfound.GetDiff(),
                                width: imgfound.Width,
                                height: imgfound.Height,
                                size: imgfound.Size,
                                id: imgfound.Id,
                                lastid: imgfound.LastId,
                                distance: imgfound.Distance);

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

                    ImageHelper.ComputeBlob(bitmap, out var descriptors);
                    if (descriptors == null || descriptors.Length == 0) {
                        message = "not enough descriptors";
                        ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {name}: {message}");
                        bad++;
                        File.Move(filename, $"{filename}{AppConsts.CorruptedExtension}");
                        continue;
                    }

                    var blob = ImageHelper.ArrayFrom64(descriptors);

                    ImageHelper.ComputePBlob(bitmap, out var hashes);
                    var pblob = ImageHelper.ArrayFrom64(hashes);

                    var len = 8;
                    while (len <= 32) {
                        name = hash.Substring(0, len);
                        if (!_imgList.ContainsKey(name)) {
                            break;
                        }

                        len++;
                    }

                    var id = AllocateId();
                    var img = new Img(
                        name: name,
                        folder: folder,
                        hash: hash,
                        blob: blob,
                        pblob: pblob,
                        lastchanged: lastchanged,
                        lastview: lastview,
                        counter: 0,
                        lastcheck: lastcheck,
                        nexthash: hash,
                        diff: new byte[1] { 0xFF },
                        width: bitmap.Width,
                        height: bitmap.Height,
                        size: imagedata.Length,
                        id: id,
                        lastid: 0,
                        distance: 256);

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
