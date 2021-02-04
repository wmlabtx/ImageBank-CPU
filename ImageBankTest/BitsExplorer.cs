using ImageBank;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ImageBankTest
{
    public class Point
    {
        public byte[] Blob { get; private set; }

        private ulong[] _descriptors;
        public ulong[] GetDescriptors()
        {
            return _descriptors;
        }

        public Point(byte[] blob)
        {
            Blob = blob;
            _descriptors = ImageHelper.ComputeDescriptors(blob);
        }

        public int GetDistance(Point other)
        {
            var d =
                Intrinsic.PopCnt(_descriptors[0] ^ other._descriptors[0]) +
                Intrinsic.PopCnt(_descriptors[1] ^ other._descriptors[1]) +
                Intrinsic.PopCnt(_descriptors[2] ^ other._descriptors[2]) +
                Intrinsic.PopCnt(_descriptors[3] ^ other._descriptors[3]);

            return d;
        }
    }

    [TestClass()]
    public class BitsExplorer
    {
        [TestMethod()]
        public void Execute()
        {
            var br1 = new byte[32];
            var p1 = new Point(br1);
            var br2 = new byte[32];
            for (var i = 0; i < br2.Length; i++)
            {
                br2[i] = 0xFF;
            }

            var p2 = new Point(br2);
            var d1 = p1.GetDistance(p1);
            Assert.AreEqual(d1, 0);
            var d2 = p1.GetDistance(p2);
            Assert.AreEqual(d2, 256);

            const int mindistance = 74;
            var list = new List<Point>();
            var counter = 0;
            var images = 0;
            using (var writer = new BinaryWriter(File.Open("clusters.dat", FileMode.Create)))
            {
                var connectionString = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={AppConsts.FileDatabase};Connection Timeout=60";
                var _sqlConnection = new SqlConnection(connectionString);
                _sqlConnection.Open();
                var sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append($"{AppConsts.AttrDescriptors} "); // 0
                sb.Append($"FROM {AppConsts.TableImages}");
                var sqltext = sb.ToString();
                {
                    using (var sqlCommand = new SqlCommand(sqltext, _sqlConnection))
                    {
                        using (var reader = sqlCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                images++;
                                var blob = (byte[])reader[0];
                                var offset = 0;
                                while (offset < blob.Length)
                                {
                                    var descriptor = new byte[32];
                                    Buffer.BlockCopy(blob, offset, descriptor, 0, 32);
                                    var point = new Point(descriptor);
                                    counter++;
                                    if (list.Count == 0)
                                    {
                                        list.Add(point);
                                    }
                                    else
                                    {
                                        var mind = 256;
                                        foreach (var e in list)
                                        {
                                            var d = e.GetDistance(point);
                                            mind = Math.Min(mind, d);
                                            if (mind < mindistance)
                                            {
                                                break;
                                            }
                                        }

                                        if (mind >= mindistance)
                                        {
                                            if (list.Count < 0x10000)
                                            {
                                                list.Add(point);
                                                writer.Write(descriptor, 0, 32);
                                            }
                                            else
                                            {
                                                Debug.WriteLine($"{mindistance} | {images} | {list.Count} | {counter}");
                                                return;
                                            }
                                        }
                                    }

                                    offset += 32;
                                }
                            }
                        }
                    }
                }

                _sqlConnection.Close();
            }

            Debug.WriteLine($"{mindistance} | {images} | {list.Count} | {counter}");
        }
    }
}
