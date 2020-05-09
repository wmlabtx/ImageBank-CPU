using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {        
        public void Import()
        { 
            AppVars.SuspendEvent.Reset();

            var added = 0;
            var found = 0;
            var bad = 0;
            var dt = DateTime.Now;

            ((IProgress<string>)AppVars.Progress).Report($"importing...");
            var directoryInfo = new DirectoryInfo(AppConsts.PathCollection);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            foreach (var fileInfo in fileInfos) {
                var filename = fileInfo.FullName;
                var shortfilename = filename.Substring(AppConsts.PathCollection.Length);

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    ((IProgress<string>)AppVars.Progress).Report($"{shortfilename} (a:{added}/f:{found}/b:{bad})...");
                }

                var name = Path.GetFileNameWithoutExtension(filename);
                lock (_imglock) {
                    if (_imgList.ContainsKey(name)) {
                        continue;
                    }
                }

                var directory = Path.GetDirectoryName(filename);
                var folder = directory.Substring(AppConsts.PathCollection.Length);
                var lastmodified = File.GetLastWriteTime(filename);

                if (!Helper.GetImageDataFromFile(
                    filename,
                    out byte[] imgdata,
#pragma warning disable CA2000 // Dispose objects before losing scope
                    out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                    out string id,
                    out string message)) {
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    return;
                }

                var lastview = GetMinLastView();
                lock (_imglock) {
                    if (_imgList.TryGetValue(id, out Img imgfound)) {
                        found++;

                        if (imgfound.Folder.Equals(folder, StringComparison.OrdinalIgnoreCase)) {
                            Helper.DeleteToRecycleBin(filename);
                            continue;
                        }
                        else {
                            Delete(imgfound.Id);
                            lastview = imgfound.LastView;
                        }
                    }
                }

                if (!OrbHelper.Compute(bitmap, out byte[] vector)) {
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    return;
                }

                bitmap.Dispose();

                var lastcheck = GetMinLastCheck();
                var img = new Img(
                    id: id,
                    folder: folder,
                    lastview: lastview,
                    nextid: string.Empty,
                    distance: 256f,
                    lastcheck: lastcheck,
                    lastmodified: lastmodified,
                    vector: vector,
                    counter: 0);

                Add(img);
                if (!filename.Equals(img.FileName, StringComparison.OrdinalIgnoreCase)) {
                    File.WriteAllBytes(img.FileName, imgdata);
                    File.SetLastWriteTime(img.FileName, lastmodified);
                    Helper.DeleteToRecycleBin(filename);
                }

                if (FindNext(id, out var nextid, out var distance)) {
                    img.NextId = nextid;
                    img.Distance = distance;
                    img.LastCheck = DateTime.Now;
                }

                if (_imgList.Count >= AppConsts.MaxImages) {
                    break;
                }

                added++;
            }

            ((IProgress<string>)AppVars.Progress).Report($"clean-up...");
            Helper.CleanupDirectories(AppConsts.PathCollection, AppVars.Progress);

            AppVars.SuspendEvent.Set();
        }
    }
}
