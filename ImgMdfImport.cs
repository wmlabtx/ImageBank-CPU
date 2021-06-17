using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void Import(int maxadd)
        { 
            AppVars.SuspendEvent.Reset();

            var added = 0;
            var bad = 0; 
            var found = 0;
            var moved = 0;
            var dt = DateTime.Now;
            var fileinfos = new List<FileInfo>();
            ((IProgress<string>)AppVars.Progress).Report($"importing {AppConsts.PathHp}...");
            var directoryInfo = new DirectoryInfo(AppConsts.PathHp);
            var fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            fileinfos.AddRange(fs);
            //((IProgress<string>)AppVars.Progress).Report($"importing {AppConsts.PathMz}...");
            //directoryInfo = new DirectoryInfo(AppConsts.PathMz);
            //fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            //fileinfos.AddRange(fs);
            ((IProgress<string>)AppVars.Progress).Report($"importing {AppConsts.PathRw}...");
            directoryInfo = new DirectoryInfo(AppConsts.PathRw);
            fs = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToArray();
            fileinfos.AddRange(fs);
            fileinfos = fileinfos.OrderBy(e => e.Length).ToList();
            foreach (var fileinfo in fileinfos) {
                var orgfilename = fileinfo.FullName;
                var orgextension = Path.GetExtension(orgfilename);
                if (orgextension.Equals(AppConsts.CorruptedExtension, StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                var orgname = Path.GetFileNameWithoutExtension(orgfilename);
                if (fileinfo.Attributes.HasFlag(FileAttributes.Hidden)) {
                    continue;
                }

                lock (_imglock) {
                    if (_imgList.ContainsKey(orgfilename)) {
                        continue;
                    }
                }

                if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                    dt = DateTime.Now;
                    ((IProgress<string>)AppVars.Progress).Report($"{orgfilename} (a:{added}/b:{bad}/f:{found}/m:{moved})...");
                }

                var path = Path.GetDirectoryName(orgfilename);

                if (!ImageHelper.GetImageDataFromFile(
                    orgfilename,
                    out var filename,
                    out var imagedata,
                    out var bitmap,
                    out var message)) {
                    ((IProgress<string>)AppVars.Progress).Report($"Corrupted image: {orgfilename}: {message}");
                    bad++;
                    continue;
                }

                var hash = Helper.ComputeHash(imagedata);
                lock (_imglock) {

                    if (_hashList.TryGetValue(hash, out var imgfound)) {

                        // we found the same image in a database

                        bitmap.Dispose();
                        
                        if (path.StartsWith(AppConsts.PathHp, StringComparison.OrdinalIgnoreCase) ||
                            path.StartsWith(AppConsts.PathRw, StringComparison.OrdinalIgnoreCase)) {

                            // the new image is coming from a heap

                            if (File.Exists(imgfound.FileName)) {

                                // no reason to add the same image from a heap; we have one

                                found++;
                                Helper.DeleteToRecycleBin(filename);
                                continue;
                            }
                            else {

                                // found image is gone; restore underlayed file only

                                var dir = Path.GetDirectoryName(imgfound.FileName);
                                if (!Directory.Exists(dir)) {
                                    Directory.CreateDirectory(dir);
                                }

                                File.Move(filename, imgfound.FileName);
                                continue;
                            }
                        }
                        else {

                            // the new image is coming from a collection;

                            if (File.Exists(imgfound.FileName) && imgfound.FileName.StartsWith(AppConsts.PathMz, StringComparison.OrdinalIgnoreCase)) {

                                // we have the same image but in other collection; we will not delete original file

                                ((IProgress<string>)AppVars.Progress).Report($"{orgfilename} = {imgfound.FileName}");
                                //AppVars.SuspendEvent.Set();
                                //return;

                                found++;
                                continue;
                            }
                            else {

                                // we will delete existing image but save its properties

                                moved++;
                                var mimg = new Img(
                                    filename: filename,
                                    other: imgfound);

                                Delete(imgfound.FileName);
                                Add(mimg);
                                continue;
                            }
                        }
                    }
                    else {

                        // it is a new image; we don't have one 

                        if (_imgList.Count >= AppConsts.MaxImages) {
                            break;
                        }

                        ImageHelper.ComputeKazeDescriptors(bitmap, out var kazeone, out var kazetwo);
                        if (kazeone == null || kazeone.Length == 0) {
                            ((IProgress<string>)AppVars.Progress).Report($"Not enough orbdescriptors: {filename}: {message}");
                            bad++;
                            File.Move(filename, $"{filename}{AppConsts.CorruptedExtension}");
                            continue;
                        }

                        var lc = GetMinLastCheck();
                        var lv = GetMinLastView();

                        if (path.StartsWith(AppConsts.PathRw, StringComparison.OrdinalIgnoreCase)) {

                            // we have to create unique name and a location in Hp folder

                            var hpsubfolder = Helper.GetHpSubFolder();
                            var newfilename = filename;
                            do {
                                var randomname = Helper.GetRandomName();
                                newfilename = $"{AppConsts.PathHp}\\{hpsubfolder}\\{randomname}{AppConsts.JpgExtension}";
                            }
                            while (File.Exists(newfilename));

                            var dir = $"{AppConsts.PathHp}\\{hpsubfolder}";
                            if (!Directory.Exists(dir)) {
                                Directory.CreateDirectory(dir);
                            }

                            added++;
                            var nimg = new Img(
                                filename: newfilename,
                                hash: hash,
                                width: bitmap.Width,
                                height: bitmap.Height,
                                size: imagedata.Length,
                                kazeone: kazeone,
                                kazetwo: kazetwo,
                                nexthash: hash,
                                kazematch: 0,
                                lastchanged: lc,
                                lastview: lv,
                                lastcheck: lc,
                                counter: 0);

                            Add(nimg);

                            var lastmodified = File.GetLastWriteTime(filename);
                            if (lastmodified > DateTime.Now) {
                                lastmodified = DateTime.Now;
                            }

                            File.WriteAllBytes(newfilename, imagedata);
                            File.SetLastWriteTime(newfilename, lastmodified);
                            if (!filename.Equals(newfilename, StringComparison.OrdinalIgnoreCase)) {
                                Helper.DeleteToRecycleBin(filename);
                            }
                        }
                        else {

                            // we are adding new image to a database

                            added++;
                            var nimg = new Img(
                                filename: filename,
                                hash: hash,
                                width: bitmap.Width,
                                height: bitmap.Height,
                                size: imagedata.Length,
                                kazeone: kazeone,
                                kazetwo: kazetwo,
                                nexthash: hash,
                                kazematch: 0,
                                lastchanged: lc,
                                lastview: lv,
                                lastcheck: lc,
                                counter: 0);

                            Add(nimg);
                        }

                        bitmap.Dispose();
                        if (added >= maxadd) {
                            break;
                        }
                    }
                }
            }

            ((IProgress<string>)AppVars.Progress).Report($"clean-up {AppConsts.PathHp}...");
            Helper.CleanupDirectories(AppConsts.PathHp, AppVars.Progress);
            ((IProgress<string>)AppVars.Progress).Report($"clean-up {AppConsts.PathMz}...");
            Helper.CleanupDirectories(AppConsts.PathMz, AppVars.Progress);
            ((IProgress<string>)AppVars.Progress).Report($"clean-up {AppConsts.PathRw}...");
            Helper.CleanupDirectories(AppConsts.PathRw, AppVars.Progress);

            AppVars.SuspendEvent.Set();
        }
    }
}
