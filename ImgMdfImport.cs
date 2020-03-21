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

                ulong[] vector;

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

                var person = string.Empty;
                var sp = Path.GetFileNameWithoutExtension(filename).Split('@');
                if (sp.Length == 2) {
                    person = sp[1];
                }

                lock (_imglock) {
                    var idchecksum = SqlGetIdByChecksum(checksum);
                    if (idchecksum > 0) {
                        if (_imgList.TryGetValue(idchecksum, out Img imgchecksum)) {
                            if (!imgchecksum.Person.Equals(person, StringComparison.OrdinalIgnoreCase)) {
                                imgchecksum.Person = person;
                            }

                            found++;
                            Helper.DeleteToRecycleBin(filename);
                            continue;
                        }
                    }
                }

                if (!OrbHelper.ComputeOrbs(bitmap, out vector)) {
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
                    sim: 0f,
                    lastcheck: new DateTime(1997, 1, 1, 8, 0, 0),
                    vector: vector,
                    format: format,
                    counter: 0);

                Add(img);
                if (!filename.Equals(img.FileName, StringComparison.OrdinalIgnoreCase)) {
                    Helper.WriteData(img.FileName, imgdata);
                    Helper.DeleteToRecycleBin(filename);
                }

                //FindNext(id, out var nextid, out var sim);
                //img.NextId = nextid;
                //img.Sim = sim;

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
            Import(100000, progress);
        }
    }
}
