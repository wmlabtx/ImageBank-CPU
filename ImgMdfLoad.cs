using System;
using System.Data.SqlClient;
using System.Text;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void LoadImgs(IProgress<string> progress)
        {
            progress.Report("Loading model...");

            lock (_imglock)
            {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrName}, "); // 0
            sb.Append($"{AppConsts.AttrFolder}, "); // 1
            sb.Append($"{AppConsts.AttrHash}, "); // 2
            sb.Append($"{AppConsts.AttrDescriptors}, "); // 3
            sb.Append($"{AppConsts.AttrPhash}, "); // 4
            sb.Append($"{AppConsts.AttrLastAdded}, "); // 5
            sb.Append($"{AppConsts.AttrLastView}, "); // 6
            sb.Append($"{AppConsts.AttrCounter}, "); // 7
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 8
            sb.Append($"{AppConsts.AttrNextHash}, "); // 9
            sb.Append($"{AppConsts.AttrDistance}, "); // 10
            sb.Append($"{AppConsts.AttrWidth}, "); // 11
            sb.Append($"{AppConsts.AttrHeight}, "); // 12
            sb.Append($"{AppConsts.AttrSize} "); // 13

            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock)
            {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection))
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                {
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        var dtn = DateTime.Now;
                        while (reader.Read())
                        {
                            var name = reader.GetString(0);
                            var folder = reader.GetString(1);
                            var hash = reader.GetString(2);
                            var blob = (byte[])reader[3];
                            var phashbuffer = (byte[])reader[4];
                            var phash = BitConverter.ToUInt64(phashbuffer, 0);
                            var lastadded = reader.GetDateTime(5);
                            var lastview = reader.GetDateTime(6);
                            var counter = reader.GetInt32(7);
                            var lastcheck = reader.GetDateTime(8);
                            var nexthash = reader.GetString(9);
                            var distance = reader.GetFloat(10);
                            var width = reader.GetInt32(11);
                            var height = reader.GetInt32(12);
                            var size = reader.GetInt32(13);
                            var img = new Img(
                                name: name,
                                folder: folder,
                                hash: hash,
                                blob: blob,
                                phash: phash,
                                lastadded: lastadded,
                                lastview: lastview,
                                counter: counter,
                                lastcheck: lastcheck,
                                nexthash: nexthash,
                                distance: distance,
                                width: width,
                                height: height,
                                size: size
                               );

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse)
                            {
                                dtn = DateTime.Now;
                                progress.Report($"Loading images ({_imgList.Count})...");
                            }
                        }
                    }
                }
            }

            progress.Report("Database loaded");
            /*
            lock (_sqllock)
            {
                var dt = DateTime.Now;
                var counter = 0;
                foreach (var name in _imgList.Keys)
                {
                    counter++;
                    if (_imgList.TryGetValue(name, out var img))
                    {
                        if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse)
                        {
                            dt = DateTime.Now;
                            progress.Report($"{counter}: {img.Folder:D2}\\{img.Name}");
                        }

                        if (img.Width == 0 || img.Height == 0 || img.Size == 0)
                        {
                            if (ImageHelper.GetImageDataFromFile(img.FileName,
                                out byte[] imagedata,
                                out var bitmap,
                                out _))
                            {
                                img.Width = bitmap.Width;
                                img.Height = bitmap.Height;
                                img.Size = imagedata.Length;
                            }
                        }
                    }
                }
            }
            */
            /*
            lock (_sqllock)
            {
                var dt = DateTime.Now;
                var counter = 0;
                var h = new int[100];
                foreach (var name in _imgList.Keys)
                {
                    counter++;
                    if (_imgList.TryGetValue(name, out var img))
                    {
                        if (DateTime.Now.Subtract(dt).TotalMilliseconds > AppConsts.TimeLapse)
                        {
                            dt = DateTime.Now;
                            progress.Report($"{counter}: {img.Folder:D2}\\{img.Name}");
                        }

                        if (img.Width != 0 && img.Height != 0 && img.Size != 0)
                        {
                            var r = 0;
                            if (img.Width >= img.Height)
                            {
                                r = img.Height * 99 / img.Width;
                            }
                            else
                            {
                                r = img.Width * 99 / img.Height;
                            }

                            h[r]++;
                        }
                    }
                }


                var sbs = new StringBuilder();
                for (var i = 0; i < h.Length; i++)
                {
                    sbs.AppendLine($"{i}:{h[i]}, ");
                }

                var s = sbs.ToString();
            
            }
            */
        }
    }
}