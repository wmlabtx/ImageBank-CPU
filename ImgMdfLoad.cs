﻿using OpenCvSharp;
using System;
using System.Data.SqlClient;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void LoadImgs(IProgress<string> progress)
        {
            progress.Report("Loading model...");

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");

            sb.Append($"{AppConsts.AttrId}, "); // 0
            sb.Append($"{AppConsts.AttrHash}, "); // 1

            sb.Append($"{AppConsts.AttrWidth}, "); // 2
            sb.Append($"{AppConsts.AttrHeight}, "); // 3
            sb.Append($"{AppConsts.AttrSize}, "); // 4

            sb.Append($"{AppConsts.AttrAkazeCentroid}, "); // 5
            sb.Append($"{AppConsts.AttrAkazeMirrorCentroid}, "); // 6
            sb.Append($"{AppConsts.AttrAkazePairs}, "); // 7

            sb.Append($"{AppConsts.AttrLastChanged}, "); // 8
            sb.Append($"{AppConsts.AttrLastView}, "); // 9
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 10
            sb.Append($"{AppConsts.AttrNextHash}, "); // 11
            sb.Append($"{AppConsts.AttrCounter} "); // 12

            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var id = reader.GetInt32(0);
                            var hash = reader.GetString(1);

                            var width = reader.GetInt32(2);
                            var height = reader.GetInt32(3);
                            var size = reader.GetInt32(4);
                            
                            var akazecentroid = (byte[])reader[5];
                            var akazemirrorcentroid = (byte[])reader[6];
                            var akazepairs = reader.GetInt32(7);

                            var lastchanged = reader.GetDateTime(8);
                            var lastview = reader.GetDateTime(9);
                            var lastcheck = reader.GetDateTime(10);
                            var nexthash = reader.GetString(11);
                            var counter = reader.GetInt32(12);

                            var img = new Img(
                                id: id,
                                hash: hash,

                                width: width,
                                height: height,
                                size: size,

                                akazecentroid: akazecentroid,
                                akazemirrorcentroid: akazemirrorcentroid,
                                akazepairs: akazepairs,

                                lastchanged: lastchanged,
                                lastview: lastview,
                                lastcheck: lastcheck,
                                nexthash: nexthash,
                                counter: counter
                               );

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                dtn = DateTime.Now;
                                progress.Report($"Loading images ({_imgList.Count})...");
                            }
                        }
                    }
                }

                progress.Report("Loading vars...");

                _id = 0;

                sb.Length = 0;
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrId} "); // 0
                sb.Append($"FROM {AppConsts.TableVars}");
                sqltext = sb.ToString();
                lock (_sqllock) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection))
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    {
                        using (var reader = sqlCommand.ExecuteReader()) {
                            while (reader.Read()) {
                                _id = reader.GetInt32(0);
                                break;
                            }
                        }
                    }
                }

                progress.Report("Database loaded");

                /*
                lock (_imgList) {
                    foreach (var img in _imgList.Select(e => e.Value).ToArray()) {
                        if (img.Id < 9500) {
                            if (img.Counter == 0) {
                                img.LastView = DateTime.Now;
                                img.Counter = 1;
                            }
                            
                        }
                    }
                }
                */
            }
        }
    }
}