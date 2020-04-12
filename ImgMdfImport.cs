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

            Helper.CleanupDirectories(AppConsts.PathCollection, progress);

            var added = 0;
            var found = 0;
            var bad = 0;
            var dt = DateTime.Now;

            progress?.Report($"importing...");
            var directoryInfo = new DirectoryInfo(AppConsts.PathCollection);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            foreach (var fileInfo in fileInfos) {
                if (added >= maxadd) {
                    break;
                }

                var filename = fileInfo.FullName;
                var shortfilename = filename.Substring(AppConsts.PathCollection.Length);

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    progress?.Report($"{shortfilename} (a:{added}/f:{found}/b:{bad})...");
                }

                var fid = Helper.GetId(filename);
                if (fid > 0) {
                    if (Helper.IsNativePath(filename)) {
                        continue;
                    }
                }

                if (!Helper.GetImageDataFromFile(
                    filename,
                    out var imgdata,
                    out var magicformat,
#pragma warning disable CA2000 // Dispose objects before losing scope
                    out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                    out var checksum,
                    out var message,
                    out var bitmapchanged)) {
                    progress?.Report($"Corrupted image: {shortfilename}: {message}");

                    if (message.Equals("too big image", StringComparison.OrdinalIgnoreCase)) {
                        var filejpg = Path.ChangeExtension(filename, AppConsts.JpgExtension);
                        if (!filejpg.Equals(filename, StringComparison.OrdinalIgnoreCase)) {
                            File.WriteAllBytes(filejpg, imgdata);
                            Helper.DeleteToRecycleBin(filename);
                        }
                    }

                    bad++;
                    continue;
                }

                lock (_imglock) {
                    var idchecksum = SqlGetIdByChecksum(checksum);
                    if (idchecksum > 0) {
                        if (_imgList.TryGetValue(idchecksum, out Img imgchecksum)) {
                            found++;
                            imgchecksum.LastAdded = DateTime.Now;
                            Helper.DeleteToRecycleBin(filename);
                            continue;
                        }
                    }
                }

                var scd = ScdHelper.Compute(bitmap);
                bitmap.Dispose();

                var id = AllocateId();
                var lastview = GetMinLastView();
                var lastcheck = GetMinLastCheck();
                var img = new Img(
                    id: id,
                    checksum: checksum,
                    lastview: lastview,
                    nextid: 0,
                    distance: 0f,
                    lastcheck: lastcheck,
                    lastadded: DateTime.Now,
                    vector: scd,
                    format: magicformat,
                    counter: 0);

                Add(img);
                if (!filename.Equals(img.FileName, StringComparison.OrdinalIgnoreCase)) {
                    Helper.WriteData(img.FileName, imgdata);
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

            Helper.CleanupDirectories(AppConsts.PathCollection, progress);

            AppVars.SuspendEvent.Set();
        }

        public void Import(IProgress<string> progress)
        {
            Contract.Requires(progress != null);
            Import(100, progress);
        }
    }
}
