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
            var moved = 0;
            var bad = 0;
            var dt = DateTime.Now;

            var directoryInfo = new DirectoryInfo(AppConsts.PathCollection);
            var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();
            foreach (var fileInfo in fileInfos) {
                var filename = fileInfo.FullName;
                var shortfilename = filename.Substring(AppConsts.PathCollection.Length);
                var name = Helper.GetName(filename);
                var idfound = SqlGetIdByName(name);
                if (idfound > 0) {
                    continue;
                }

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    progress?.Report($"{shortfilename} (a:{added}/f:{found}/m:{moved}/b:{bad})...");
                }

                //var extension = Helper.GetExtension(filename);
                if (!Helper.GetImageDataFromFile(
                    filename,
                    out var imgdata,
#pragma warning disable CA2000 // Dispose objects before losing scope
                    out Bitmap bitmap,
#pragma warning restore CA2000 // Dispose objects before losing scope
                    out var checksum,
                    out var message)) {
                    progress?.Report($"Corrupted image: {shortfilename}: {message}");
                    bad++;
                    continue;
                }

                var prefixname = Helper.GetPrefixName(filename);
                if (string.IsNullOrEmpty(prefixname)) {
                    progress?.Report($"Wrong location: {shortfilename}");
                    return;
                }

                string suggestedname;
                string suggestedfilename;
                lock (_imglock) {
                    var idchecksum = SqlGetIdByChecksum(checksum);
                    if (idchecksum > 0) {
                        if (_imgList.TryGetValue(idchecksum, out var imgfound)) {
                            if (imgfound.File.Equals(filename, StringComparison.OrdinalIgnoreCase)) {
                                continue;
                            }

                            if (imgfound.Name.StartsWith(AppConsts.PrefixLegacy, StringComparison.OrdinalIgnoreCase)) {
                                var path = Path
                                    .GetDirectoryName(filename)
                                    .Substring(AppConsts.PathCollection.Length);

                                if (path.StartsWith(AppConsts.PrefixLegacy, StringComparison.OrdinalIgnoreCase)) {
                                    found++;
                                }
                                else {
                                    moved++;
                                    suggestedname = GetSuggestedName(prefixname, checksum);
                                    suggestedfilename = Helper.GetFileName(suggestedname);
                                    Helper.WriteData(suggestedfilename, imgdata);
                                    Helper.DeleteToRecycleBin(imgfound.File);
                                    imgfound.Name = suggestedname;
                                }
                            }
                            else {
                                found++;
                            }

                            Helper.DeleteToRecycleBin(filename);
                            continue;

                        }
                    }
                }

                if (!Helper.GetVector(bitmap, out var vector)) {
                    progress?.Report($"Cannot get descriptors: {shortfilename}");
                    bad++;
                    continue;
                }

                bitmap.Dispose();

                suggestedname = GetSuggestedName(prefixname, checksum);
                suggestedfilename = Helper.GetFileName(suggestedname);
                var generation = suggestedname.StartsWith(AppConsts.PrefixLegacy, StringComparison.OrdinalIgnoreCase) ?
                    1 : 0;

                var id = AllocateId();
                var lastview = GetMinLastView();
                var img = new Img(
                    id: id,
                    name: suggestedname,
                    checksum: checksum,
                    generation: generation,
                    lastview: lastview,
                    nextid: id,
                    distance: 1f,
                    lastid: 0,
                    lastchange: lastview,
                    lastfind: lastview,
                    vector: vector);

                Add(img);
                if (!filename.Equals(img.File, StringComparison.OrdinalIgnoreCase)) {
                    Helper.WriteData(suggestedfilename, imgdata);
                    Helper.DeleteToRecycleBin(filename);
                }

                /*
                FindNext(id, out var lastid, out var lastchange, out var nextid, out var distance);
                img.LastId = lastid;
                img.LastChange = lastchange;
                img.NextId = nextid;
                img.Distance = distance;
                */

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

        public void Convert(IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            /*
            AppVars.SuspendEvent.Reset();

            var dt = DateTime.Now;
            lock (_imglock) {
                var images = _imgList.Values.ToArray();
                foreach (var img in images) {
                    if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                        dt = DateTime.Now;
                        progress?.Report($"{img.Path}\\{img.Name}...");
                    }

                    var filename = img.OldFile;
                    if (!File.Exists(filename)) {
                        continue;
                    }

                    var imgdata = File.ReadAllBytes(filename);
                    var dir = img.Id % 100;
                    string suggestedpath = $"legacy\\{dir:D2}";
                    string prefix = $"legacy.{dir:D2}.";
                    string suggestedname;
                    var namelenght = 2;
                    do {
                        namelenght++;
                        suggestedname = string.Concat(prefix, img.Checksum.Substring(0, namelenght));
                    } while (_nameList.ContainsKey(suggestedname));

                    var suggestedfilename = $"{AppConsts.PathCollection}{suggestedpath}\\{suggestedname}{AppConsts.MzxExtension}";
                    var encdata = Helper.Encrypt(imgdata, suggestedname);
                    var directory = Path.GetDirectoryName(suggestedfilename);
                    if (!Directory.Exists(directory)) {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllBytes(suggestedfilename, encdata);
                    Helper.DeleteToRecycleBin(filename);
                    img.Name = suggestedname;
                    img.Path = suggestedpath;
                }
            }

            AppVars.SuspendEvent.Set();
            */
        }
    }
}
