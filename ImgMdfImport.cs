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
                var extension = Path.GetExtension(filename);
                if (extension.Equals(AppConsts.CorruptedExtension)) {
                    continue;
                }

                string shortfilename;
                if (path.Equals(AppConsts.PathHp)) {
                    shortfilename = filename.Substring(AppConsts.PathHp.Length + 1);
                    folder = Path.GetDirectoryName(shortfilename);
                    if (int.TryParse(folder, out var foundfolder)) {
                        var name = Path.GetFileNameWithoutExtension(filename);
                        if (int.TryParse(name, out var foundid)) {
                            if (_imgList.TryGetValue(foundid, out var imgfound)) {
                                if (foundfolder == imgfound.Folder) {
                                    continue;
                                }

                                if (File.Exists(imgfound.FileName)) {
                                    found++;
                                    Helper.DeleteToRecycleBin(filename);
                                    continue;
                                }

                                Delete(imgfound.Id);
                            }
                        }
                    }
                }

                shortfilename = filename.Substring(AppConsts.PathRoot.Length);

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    ((IProgress<string>)AppVars.Progress).Report($"{shortfilename} (a:{added}/b:{bad}/f:{found}/m:{moved})...");
                }

                if (!ImageHelper.GetImageDataFromFile(
                    filename,
                    out var imagedata,
                    out var bitmap,
                    out var message)) {
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    File.Move(filename, $"{filename}{AppConsts.CorruptedExtension}");
                    continue;
                }

                lock (_imglock) {
                    var lastchanged = DateTime.Now;
                    var lastview = new DateTime(2020, 1, 1);
                    var lastcheck = GetMinLastCheck();
                    var hash = Helper.ComputeHash(imagedata);

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
                                hash: imgfound.Hash,

                                width: imgfound.Width,
                                height: imgfound.Height,
                                size: imgfound.Size,

                                akazepairs: imgfound.AkazePairs,
                                akazedescriptors: imgfound.AkazeDescriptors,
                                akazemirrordescriptors: imgfound.AkazeMirrorDescriptors,

                                lastchanged: lastchanged,
                                lastview: lastview,
                                lastcheck: lastcheck,

                                nexthash: imgfound.NextHash,
                                counter: imgfound.Counter);

                            var lastmodifiedfound = File.GetLastWriteTime(imgfound.FileName);
                            Helper.WriteData(imgreplace.FileName, imagedata);
                            File.SetLastWriteTime(imgreplace.FileName, lastmodifiedfound);
                            Helper.DeleteToRecycleBin(filename);

                            Delete(imgfound.Id);
                            Add(imgreplace);
                            bitmap.Dispose();

                            moved++;
                        }

                        continue;
                    }

                    ImageHelper.ComputeAkazeDescriptors(bitmap, out var akazedescriptors, out var akazemirrordescriptors);
                    if (akazedescriptors == null || akazedescriptors.Rows == 0) {
                        ((IProgress<string>)AppVars.Progress).Report($"Not enough orbdescriptors: {shortfilename}: {message}");
                        bad++;
                        File.Move(filename, $"{filename}{AppConsts.CorruptedExtension}");
                        continue;
                    }

                    var id = AllocateId();
                    var img = new Img(
                        id: id,
                        hash: hash,

                        width: bitmap.Width,
                        height: bitmap.Height,
                        size: imagedata.Length,

                        akazepairs: 0,
                        akazedescriptors: akazedescriptors,
                        akazemirrordescriptors: akazemirrordescriptors,

                        lastchanged: lastchanged,
                        lastview: lastview,
                        lastcheck: lastcheck,

                        nexthash: hash,
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
