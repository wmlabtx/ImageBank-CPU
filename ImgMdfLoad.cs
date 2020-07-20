using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void Load(IProgress<string> progress)
        {
            Contract.Requires(progress != null);

            progress.Report("Loading model...");

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var imgtodelete = new List<string>();
            var imgtoupdate = new List<string>();

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrName}, "); // 0
            sb.Append($"{AppConsts.AttrFolder}, "); // 1
            sb.Append($"{AppConsts.AttrPath}, "); // 2
            sb.Append($"{AppConsts.AttrHash}, "); // 3
            sb.Append($"{AppConsts.AttrPHash}, "); // 4
            sb.Append($"{AppConsts.AttrCounter}, "); // 5
            sb.Append($"{AppConsts.AttrLastView}, "); // 6
            sb.Append($"{AppConsts.AttrWidth}, "); // 7
            sb.Append($"{AppConsts.AttrHeigth}, "); // 8
            sb.Append($"{AppConsts.AttrSize}, "); // 9
            sb.Append($"{AppConsts.AttrDescriptors}, "); // 10
            sb.Append($"{AppConsts.AttrScd}, "); // 11
            sb.Append($"{AppConsts.AttrLastAdded} "); // 12
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dt = DateTime.Now;
                        while (reader.Read()) {
                            var name = reader.GetString(0);
                            var folder = reader.GetInt32(1);
                            var path = reader.GetString(2);
                            var bhash = (byte[])reader[3];
                            var hash = BitConverter.ToUInt64(bhash, 0);
                            var bphash = (byte[])reader[4];
                            var phash = BitConverter.ToUInt64(bphash, 0);
                            var counter = reader.GetInt32(5);
                            var lastview = reader.GetDateTime(6);
                            var width = reader.GetInt32(7);
                            var heigth = reader.GetInt32(8);
                            var size = reader.GetInt32(9);
                            var bdescriptors = (byte[])reader[10];
                            var descriptors = Helper.BufferToDescriptors(bdescriptors);
                            var bscd = (byte[])reader[11];
                            var lastadded = reader.GetDateTime(12);
                            var scd = new Scd(bscd);

                            var file = Helper.GetFileName(name, folder);
                            if (!File.Exists(file)) {
                                imgtodelete.Add(name);
                            }
                            else {
                                if (lastadded.Equals(new DateTime(2000, 1, 1))) {
                                    imgtoupdate.Add(name);
                                }
                            }

                            var img = new Img(
                                name: name,
                                hash: hash,
                                phash: phash,
                                width: width,
                                heigth: heigth,
                                size: size,
                                scd: scd,
                                descriptors: descriptors,
                                folder: folder,
                                path: path,
                                counter: counter,
                                lastadded: lastadded,
                                lastview: lastview
                               );

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse) {
                                dt = DateTime.Now;
                                progress.Report($"Loading images ({_imgList.Count})...");
                            }
                        }
                    }
                }

                foreach (var name in imgtodelete) {
                    Delete(name);
                }

                var dtla = DateTime.Now;
                var cla = 0;
                foreach (var name in imgtoupdate) {
                    cla++;
                    if (DateTime.Now.Subtract(dtla).TotalMilliseconds > AppConsts.TimeLapse) {
                        dtla = DateTime.Now;
                        progress.Report($"Updating LA ({cla}/{imgtoupdate.Count})...");
                    }

                    var img = _imgList[name];
                    img.LastAdded = File.GetLastWriteTime(img.FileName);
                }
            }

            progress.Report("Database loaded");
        }
    }
}