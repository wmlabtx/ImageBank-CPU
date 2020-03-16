using OpenCvSharp;
using System;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        private void Import(float level, IProgress<string> progress)
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
                var filename = fileInfo.FullName;
                var shortfilename = filename.Substring(AppConsts.PathCollection.Length);

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    float fill;
                    lock (_imglock) {
                        fill = _imgList.Sum(e => e.Value.LastId) * 100f / (_imgList.Count * (float)_id);
                        if (fill < level) {
                            break;
                        }
                    }

                    progress?.Report($"f={fill:F2} {shortfilename} (a:{added}/f:{found}/b:{bad})...");
                }

                var fid = Helper.GetId(filename);
                if (fid > 0) {
                    if (Helper.IsNativePath(filename)) {
                        continue;
                    }
                }

                Mat vector;
                ulong[] scalar;

                if (!Helper.GetImageDataFromFile(
                    filename,
                    out var imgdata,
#pragma warning disable CA2000 // Dispose objects before losing scope
                    out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                    out int format,
                    out var checksum,
                    out var message)) {
                    progress?.Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    continue;
                }

                var person = 0;
                var sp = Path.GetFileNameWithoutExtension(filename).Split('@');
                if (sp.Length == 2) {
                    if (!int.TryParse(sp[1], out person)) {
                        person = 0;
                    }
                }

                lock (_imglock) {
                    var idchecksum = SqlGetIdByChecksum(checksum);
                    if (idchecksum > 0) {
                        if (_imgList.TryGetValue(idchecksum, out Img imgchecksum)) {
                            if (imgchecksum.Person != person) {
                                imgchecksum.Person = person;
                            }

                            found++;
                            Helper.DeleteToRecycleBin(filename);
                            continue;
                        }
                    }
                }

                if (!OrbHelper.ComputeOrbs(bitmap, out vector, out scalar)) {
                    progress?.Report($"Cannot get descriptors: {shortfilename}");
                    bad++;
                    continue;
                }

                bitmap.Dispose();

                var id = AllocateId();
                var lastview = GetMinLastView();
                var img = new Img(
                    id: id,
                    checksum: checksum,
                    person: person,
                    lastview: lastview,
                    nextid: id,
                    distance: 256f,
                    lastid: 0,
                    vector: vector,
                    format: format,
                    scalar: scalar,
                    counter: 0);

                Add(img);
                if (!filename.Equals(img.FileName, StringComparison.OrdinalIgnoreCase)) {
                    Helper.WriteData(img.FileName, imgdata);
                    Helper.DeleteToRecycleBin(filename);
                }

                FindNext(id, out var lastid, out var nextid, out var distance);
                img.LastId = lastid;
                img.NextId = nextid;
                img.Distance = distance;

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
            Import(1f, progress);
        }
    }
}
