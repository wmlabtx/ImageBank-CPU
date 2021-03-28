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
                                id: imgfound.Id,
                                name: imgfound.Name,
                                folder: folder,
                                hash: imgfound.Hash,

                                width: imgfound.Width,
                                height: imgfound.Height,
                                size: imgfound.Size,

                                colordescriptors: imgfound.ColorDescriptors,
                                colordistance: imgfound.ColorDistance,
                                perceptivedescriptors: imgfound.PerceptiveDescriptors,
                                perceptivedistance: imgfound.PerceptiveDistance,
                                orbdescriptors: imgfound.OrbDescriptors,
                                orbdistance: imgfound.OrbDistance,

                                lastchanged: lastchanged,
                                lastview: lastview,
                                lastcheck: lastcheck,


                                nexthash: imgfound.NextHash,
                                lastid: imgfound.LastId,
                                counter: imgfound.Counter);

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

                    ImageHelper.ComputeColorDescriptors(bitmap, out var colordescriptors);
                    ImageHelper.ComputePerceptiveDescriptors(bitmap, out var perceptivedescriptors);
                    ImageHelper.ComputeOrbDescriptors(bitmap, out var orbdescriptors);
                    if (orbdescriptors.Length == 0) {
                        ((IProgress<string>)AppVars.Progress).Report($"Not enough orbdescriptors: {name}: {message}");
                        bad++;
                        File.Move(filename, $"{filename}{AppConsts.CorruptedExtension}");
                        continue;
                    }

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
                        id: id,
                        name: name,
                        folder: folder,
                        hash: hash,

                        width: bitmap.Width,
                        height: bitmap.Height,
                        size: imagedata.Length,

                        colordescriptors: colordescriptors,
                        colordistance: 100f,
                        perceptivedescriptors: perceptivedescriptors,
                        perceptivedistance: AppConsts.MaxPerceptiveDistance,
                        orbdescriptors: orbdescriptors,
                        orbdistance: AppConsts.MaxOrbDistance,

                        lastchanged: lastchanged,
                        lastview: lastview,
                        lastcheck: lastcheck,

                        nexthash: hash,
                        lastid: 0,
                        counter: 0);

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
