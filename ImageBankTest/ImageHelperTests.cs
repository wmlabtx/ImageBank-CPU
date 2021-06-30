using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageBank;
using MetadataExtractor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NExifTool;

namespace ImageBankTest
{
    [TestClass()]
    public class ImageHelperTests
    {
        /*
        private static string AsciiBytesToString(byte[] buffer, int maxlength)
        {
            for (int i = 0; i < maxlength; i++) {
                if (buffer[i] != 0) {
                    continue;
                }

                return Encoding.ASCII.GetString(buffer, 0, i);
            }

            return Encoding.ASCII.GetString(buffer, 0, maxlength);
        }

        private static async Task<IEnumerable<NExifTool.Tag>> GetExifTagsAsync(string filename)
        {
            IEnumerable<NExifTool.Tag> tags;
            try {
                var et = new ExifTool(new ExifToolOptions());
                tags = await et.GetTagsAsync(filename);
            }
            catch {
                tags = null;
            }

            return tags;
        }
        */

        [TestMethod()]
        public void ComputeKazeTestAsync()
        {
            var filename = "k1024.jpg";
            using (var image = Image.FromFile(filename)) {
                ImageHelper.ComputeKazeDescriptors((Bitmap)image, out var b1, out var bm1);
            }

            /*
            DateTime? datetaken = null;
            var task = GetExifTagsAsync(filename);
            task.Wait();
            var tags = task.Result;

            try {
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    var bitmapframe = BitmapFrame.Create(fs);
                    BitmapMetadata md = (BitmapMetadata)bitmapframe.Metadata;
                    var sdate = md.DateTaken;
                    if (sdate != null) {
                        if (DateTime.TryParse(sdate, out var dt)) {
                            datetaken = dt;
                        }
                    }
                }
            }
            catch {
            }

            if (datetaken == null) {
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (var myImage = Image.FromStream(fs, false, false)) {
                    foreach (var property in myImage.PropertyItems) {
                        if (property.Id == 0x0132) {
                            var sdt = AsciiBytesToString(property.Value, 20);
                            if (DateTime.TryParseExact(sdt, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) {
                                datetaken = dt;
                            }
                        }
                    }
                }
            }

            try {
                var directories = ImageMetadataReader.ReadMetadata(filename);
                if (directories != null && directories.Count > 0) {
                    var sb = new StringBuilder();
                    foreach (var directory in directories) {
                        if (directory.Name.Equals("File type", StringComparison.OrdinalIgnoreCase) ||
                            directory.Name.Equals("File", StringComparison.OrdinalIgnoreCase) ||
                            directory.Name.Equals("JPEG", StringComparison.OrdinalIgnoreCase) ||
                            directory.Name.Equals("JFIF", StringComparison.OrdinalIgnoreCase) ||
                            directory.Name.Equals("Huffman", StringComparison.OrdinalIgnoreCase)
                            ) {
                            continue;
                        }

                        foreach (var tag in directory.Tags) {
                            // [ICC Profile] Profile Date/Time - 1998:02:09 06:...
                            if (datetaken == null) {
                                if (directory.Name.Equals("ICC Profile", StringComparison.OrdinalIgnoreCase) &&
                                    tag.HasName && tag.Name.Equals("Profile Date/Time", StringComparison.OrdinalIgnoreCase)) {
                                    var sdt = tag.Description;
                                    if (sdt != null) {
                                        if (DateTime.TryParseExact(sdt, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) {
                                            datetaken = dt;
                                        }
                                    }
                                }
                            }

                            var tagstring = tag.ToString();
                            if (tagstring.Length > 48) {
                                tagstring = tagstring.Substring(0, 48) + "...";
                            }

                            sb.AppendLine(tagstring);
                        }

                        foreach (var error in directory.Errors) {
                            var tagstring = error.ToString();
                            if (tagstring.Length > 48) {
                                tagstring = tagstring.Substring(0, 48) + "...";
                            }

                            sb.AppendLine("ERROR: " + tagstring);
                        }
                    }

                    string metadata = sb.ToString();
                }
            }
            catch {
            }

            if (datetaken == null) {
                var lastmodified = File.GetLastWriteTime(filename);
                if (lastmodified > DateTime.Now || lastmodified.Year < 1991) {
                    lastmodified = DateTime.Now;
                }

                datetaken = lastmodified;
            }
            */
        }

        [TestMethod()]
        public void GetKazeBulkTest()
        {
            var image1 = Image.FromFile("org.jpg");
            ImageHelper.ComputeKazeDescriptors((Bitmap)image1, out var b1, out var bm1);

            var files = new[] {
                "org_png.jpg",
                "org_resized.jpg",
                "org_nologo.jpg",
                "org_r10.jpg",
                "org_r90.jpg",
                "org_bwresized.jpg",
                "org_compressed.jpg",
                "org_sim1.jpg",
                "org_sim2.jpg",
                "org_crop.jpg",
                "org_nosim1.jpg",
                "org_nosim2.jpg",
                "org_mirror.jpg",
                "k1024.jpg"
            };

            var sb = new StringBuilder();
            foreach (var filename in files)
            {
                var image2 = Image.FromFile(filename);
                ImageHelper.ComputeKazeDescriptors((Bitmap)image2, out var b2, out var bm2);

                var m = ImageHelper.ComputeKazeMatch(b1, b2, bm2);

                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: m={m}");
            }

            File.WriteAllText("report.txt", sb.ToString());
        }

        /*
        [TestMethod()]
        public void ComputeBits()
        {
            var counter = 0;
            var array = new int[256];
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrDescriptors} "); // 0
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=60";
            var _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.Open();
            using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection))
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            {
                using (var reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var blob = (byte[])reader[0];
                        var offset = 0;
                        while (offset < blob.Length)
                        {
                            for (var ob = 0; ob < 256; ob++)
                            {
                                var bit = blob[offset + (ob / 8)] & (1 << (ob % 8));
                                if (bit != 0)
                                {
                                    array[ob]++;
                                }
                            }

                            offset += 32;
                            counter++;
                        }

                        //if (counter > 10000)
                        //{
                        //    break;
                        //}
                    }
                }
            }

            var half = counter / 2;
            var stat = new List<Tuple<int, float>>();
            for (var i = 0; i < 256; i++)
            {
                var f = Math.Abs(array[i] - half) * 100f / counter;
                stat.Add(new Tuple<int, float>(i, f));
            }

            stat = stat.OrderBy(e => e.Item2).ToList();

        }
        */
    }
}