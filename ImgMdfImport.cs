using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private void Import(int maxadd, IProgress<string> progress)
        { 
            AppVars.SuspendEvent.Reset();

            var added = 0;
            var found = 0;
            var dt = DateTime.Now;

            var directoryInfo = new DirectoryInfo(AppConsts.PathCollection);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            foreach (var fileInfo in fileInfos) {
                var filename = fileInfo.FullName;
                var name = Helper.GetName(filename);
                var path = Helper.GetPath(filename);
                Img imgfound;
                lock (_imglock) {
                    if (_nameList.TryGetValue(name, out imgfound)) {
                        if (path.Equals(imgfound.Path, StringComparison.OrdinalIgnoreCase)) {
                            continue;
                        }
                    }
                }

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    var file = filename.Substring(AppConsts.PathCollection.Length);
                    progress?.Report($"{file} (a:{added}/f:{found})...");
                }

                var extension = Helper.GetExtension(filename);
                if (!Helper.GetImageDataFromFile(
                    filename,
                    out var imgdata,
#pragma warning disable CA2000 // Dispose objects before losing scope
                    out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                    out var checksum,
                    out var needwrite)) {
                    progress?.Report($"Corrupted image: {path}\\{name}{extension}");
                    return;
                }

                lock (_imglock) {
                    if (_checksumList.TryGetValue(checksum, out imgfound)) {
                        found++;
                        Helper.DeleteToRecycleBin(filename);
                        continue;
                    }
                }

                if (!Helper.GetVector(bitmap, out var vector)) {
                    progress?.Report($"Cannot get descriptors: {path}\\{name}{extension}");
                    return;
                }

                bitmap.Dispose();

                var id = AllocateId();
                string suggestedname;
                var namelenght = 2;
                lock (_imglock) {
                    do {
                        namelenght++;
                        suggestedname = string.Concat(AppConsts.PrefixName, checksum.Substring(0, namelenght));
                    } while (_nameList.ContainsKey(suggestedname));
                }

                var suggestedfilename = Helper.GetFileName(suggestedname, path);
                var lastview = GetMinLastView();
                var img = new Img(
                    id: id,
                    name: suggestedname,
                    path: path,
                    checksum: checksum,
                    generation: 1,
                    lastview: lastview,
                    nextid: id,
                    distance: 1f,
                    lastid: -1,
                    lastchange: lastview,
                    vector: vector);

                Add(img);
                if (needwrite) {
                    Helper.WriteData(suggestedfilename, imgdata);
                    Helper.DeleteToRecycleBin(filename);
                }
                else {
                    if (!filename.Equals(img.File, StringComparison.OrdinalIgnoreCase)) {
                        File.Move(filename, img.File);
                    }
                }

                FindNext(id, out var lastid, out var lastchange, out var nextid, out var distance);
                img.LastId = lastid;
                img.LastChange = lastchange;
                img.NextId = nextid;
                img.Distance = distance;

                if (_imgList.Count >= AppConsts.MaxImages) {
                    break;
                }

                added++;
                if (added >= maxadd) {
                    break;
                }
            }

            AppVars.SuspendEvent.Set();
        }

        public void Import(IProgress<string> progress)
        {
            Contract.Requires(progress != null);
            //Helper.CleanupDirectories(AppConsts.PathCollection, progress);
            Import(AppConsts.MaxImport, progress);
        }
    }
}
