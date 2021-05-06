using OpenCvSharp;
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

            lock (_imglock) {
                _imgList.Clear();
            }

            progress.Report("Loading images...");

            var sb = new StringBuilder();
            sb.Append("SELECT ");

            sb.Append($"{AppConsts.AttrId}, "); // 0
            sb.Append($"{AppConsts.AttrName}, "); // 1
            sb.Append($"{AppConsts.AttrFolder}, "); // 2
            sb.Append($"{AppConsts.AttrHash}, "); // 3

            sb.Append($"{AppConsts.AttrWidth}, "); // 4
            sb.Append($"{AppConsts.AttrHeight}, "); // 5
            sb.Append($"{AppConsts.AttrSize}, "); // 6

            sb.Append($"{AppConsts.AttrPerceptiveDescriptorsBlob}, "); // 7
            sb.Append($"{AppConsts.AttrPerceptiveDistance}, "); // 8
            
            sb.Append($"{AppConsts.AttrAkazePairs}, "); // 9
            sb.Append($"{AppConsts.AttrAkazeCentroid}, "); // 10

            sb.Append($"{AppConsts.AttrLastChanged}, "); // 11
            sb.Append($"{AppConsts.AttrLastView}, "); // 12
            sb.Append($"{AppConsts.AttrLastCheck}, "); // 13
            sb.Append($"{AppConsts.AttrNextHash}, "); // 14
            sb.Append($"{AppConsts.AttrCounter} "); // 15

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
                            var name = reader.GetString(1);
                            var folder = reader.GetString(2);
                            var hash = reader.GetString(3);

                            var width = reader.GetInt32(4);
                            var height = reader.GetInt32(5);
                            var size = reader.GetInt32(6);

                            var perceptivedescriptorsblob = (byte[])reader[7];
                            var perceptivedescriptors = ImageHelper.ArrayTo64(perceptivedescriptorsblob);
                            var perceptivedistance = reader.GetInt32(8);
                            
                            var akazepairs = reader.GetInt32(9);
                            var akazecentroid = (byte[])reader[10];

                            var lastchanged = reader.GetDateTime(11);
                            var lastview = reader.GetDateTime(12);
                            var lastcheck = reader.GetDateTime(13);
                            var nexthash = reader.GetString(14);
                            var counter = reader.GetInt32(15);

                            var img = new Img(
                                id: id,
                                name: name,
                                folder: folder,
                                hash: hash,

                                width: width,
                                height: height,
                                size: size,

                                perceptivedescriptors: perceptivedescriptors,
                                perceptivedistance: perceptivedistance,
                                akazepairs: akazepairs,
                                akazecentroid: akazecentroid,

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
            }

            progress.Report("Database loaded");
        }

        public static Mat LoadAkazeDescriptors(string name)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{AppConsts.AttrAkazeDescriptorsBlob} ");
            sb.Append($"FROM {AppConsts.TableImages} ");
            sb.Append($"WHERE {AppConsts.AttrName} = @{AppConsts.AttrName}");
            var sqltext = sb.ToString();
            lock (_sqllock) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection)) {
                    sqlCommand.Parameters.AddWithValue($"@{AppConsts.AttrName}", name);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    using (var reader = sqlCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            var akazedescriptorsblob = (byte[])reader[0];
                            var akazedescriptors = ImageHelper.ArrayToMat(akazedescriptorsblob);
                            return akazedescriptors;
                        }
                    }
                }
            }

            return null;
        }
    }
}