using System.Drawing;
using System.IO;
using System.Text;
using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBankTest
{
    [TestClass()]
    public class ImageHelperTests
    {
        [TestMethod()]
        public void ComputeBlobTest()
        {
            var image = Image.FromFile("org.jpg");
            ImageHelper.ComputeAkazeDescriptors((Bitmap)image, out var a1);
            var array = ImageHelper.ArrayFromMat(a1);
            var a2 = ImageHelper.ArrayToMat(array);
            var ob1 = a1.At<byte>(1, 2);
            var ob2 = a2.At<byte>(1, 2);
            Assert.AreEqual(ob1, ob2);
        }

        [TestMethod()]
        public void CompareBlobTest()
        {
            var image1 = Image.FromFile("org.jpg");
            ImageHelper.ComputeAkazeDescriptors((Bitmap)image1, out var a1);

            var image2 = Image.FromFile("org_nologo.jpg");
            ImageHelper.ComputeAkazeDescriptors((Bitmap)image2, out var a2);

            var p = ImageHelper.ComputeAkazePairs(a1, a2);
        }

        [TestMethod()]
        public void GetDistanceBulkTest()
        {
            var image1 = Image.FromFile("org.jpg");
            ImageHelper.ComputeAkazeDescriptors((Bitmap)image1, out var a1);

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
                ImageHelper.ComputeAkazeDescriptors((Bitmap)image2, out var a2);

                var p = ImageHelper.ComputeAkazePairs(a1, a2);
                if (sb.Length > 0) {
                    sb.AppendLine();
                }

                sb.Append($"{filename}: p={p}");
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