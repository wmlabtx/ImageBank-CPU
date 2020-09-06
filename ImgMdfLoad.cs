using System;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
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

            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrName}, "); // 0
            sb.Append($"{AppConsts.AttrFolder}, "); // 1
            sb.Append($"{AppConsts.AttrHash}, "); // 2
            sb.Append($"{AppConsts.AttrCounter}, "); // 3
            sb.Append($"{AppConsts.AttrLastView}, "); // 4
            sb.Append($"{AppConsts.AttrWidth}, "); // 5
            sb.Append($"{AppConsts.AttrHeigth}, "); // 6
            sb.Append($"{AppConsts.AttrSize}, "); // 7
            sb.Append($"{AppConsts.AttrDescriptors}, "); // 8
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 9
            sb.Append($"{AppConsts.AttrNextName} "); // 10
            sb.Append($"FROM {AppConsts.TableImages}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    using (var reader = sqlCommand.ExecuteReader()) {
                        var dtn = DateTime.Now;
                        while (reader.Read()) {
                            var name = reader.GetString(0);
                            var folder = reader.GetInt32(1);
                            var bhash = (byte[])reader[2];
                            var hash = BitConverter.ToUInt64(bhash, 0);
                            var counter = reader.GetInt32(3);
                            var lastview = reader.GetDateTime(4);
                            var width = reader.GetInt32(5);
                            var heigth = reader.GetInt32(6);
                            var size = reader.GetInt32(7);
                            var bdescriptors = (byte[])reader[8];
                            var descriptors = Helper.BufferToDescriptors(bdescriptors);
                            var lastcheck = reader.GetDateTime(9);
                            var nextname = reader.GetString(10);
                            var img = new Img(
                                name: name,
                                hash: hash,
                                width: width,
                                heigth: heigth,
                                size: size,
                                descriptors: descriptors,
                                folder: folder,
                                counter: counter,
                                lastview: lastview,
                                lastcheck: lastcheck,
                                nextname: nextname
                               );

                            AddToMemory(img);

                            if (DateTime.Now.Subtract(dtn).TotalMilliseconds > AppConsts.TimeLapse) {
                                dtn = DateTime.Now;
                                progress.Report($"Loading images ({_imgList.Count})...");
                            }
                        }
                    }
                }
            }

            progress.Report("Database loaded");
        }
    }
}