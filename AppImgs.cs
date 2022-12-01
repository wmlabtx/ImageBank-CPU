using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ImageBank
{
    public static class AppImgs
    {
        private static readonly SortedList<int, Img> _imgList = new SortedList<int, Img>();
        private static readonly SortedList<string, Img> _nameList = new SortedList<string, Img>();
        private static readonly SortedList<string, Img> _hashList = new SortedList<string, Img>();

        public static void Clear()
        {
            _imgList.Clear();
            _nameList.Clear();
            _hashList.Clear();
        }

        public static int Count()
        {
            return _imgList.Count;
        }

        public static void Add(Img img)
        {
            _imgList.Add(img.Id, img);
            _nameList.Add(img.Name, img);
            _hashList.Add(img.Hash, img);
        }

        public static bool TryGetValue(int id, out Img img)
        {
            bool result = _imgList.TryGetValue(id, out Img _img);
            img = _img;
            return result;
        }

        public static bool ContainsId(int id)
        {
            bool result = _imgList.ContainsKey(id);
            return result;
        }

        public static bool ContainsHash(string hash)
        {
            bool result = _hashList.ContainsKey(hash);
            return result;
        }

        public static bool TryGetHash(string hash, out Img img)
        {
            bool result = _hashList.TryGetValue(hash, out Img _img);
            img = _img;
            return result;
        }

        public static bool ContainsName(string name)
        {
            bool result = _nameList.ContainsKey(name);
            return result;
        }

        public static void Delete(Img img)
        {
            _nameList.Remove(img.Name);
            _hashList.Remove(img.Hash);
            _imgList.Remove(img.Id);
        }

        public static DateTime GetMinLastView()
        {
            DateTime lv = _imgList.Min(e => e.Value.LastView).AddSeconds(1);
            return lv;
        }

        public static Img GetNextView()
        {
            int id;
            Img imgX;
            var rand = AppVars.IRandom(0, 2);
            if (rand == 0) {
                rand = AppVars.IRandom(0, _imgList.Keys.Count - 1);
                id = _imgList.Keys[rand];
            }
            else {
                var minlv = _imgList.Values.Min(e => e.LastView);
                var nextday = minlv.AddDays(1);
                var oldday = _imgList.Values.Where(e => e.LastView < nextday).Select(e => e.Id).ToArray();
                rand = AppVars.IRandom(0, oldday.Length - 1);
                id = oldday[rand];
            }

            imgX = _imgList[id];
            return imgX;
        }

        public static int[] GetKeys()
        {
            int[] result;
            result = _imgList.Keys.ToArray();
            return result;
        }

        public static List<Tuple<int, float>> GetSimilars(Img imgX)
        {
            var similars = new List<Tuple<int, float>>();
            var scope = _imgList.Values.Where(e => e.Id != imgX.Id);
            foreach (var img in scope) {
                var distance = AppPalette.GetDistance(imgX.GetHist(), img.GetHist());
                if (imgX.FamilyId == 0 || imgX.FamilyId != img.FamilyId) {
                    distance += 1f;
                }

                similars.Add(Tuple.Create(img.Id, distance));
            }

            similars.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            return similars;
        }

        public static int GetFamilySize(int familyid)
        {
            var count = _imgList.Values.Count(e => e.FamilyId == familyid);
            return count;
        }

        public static void ResetFamily(Img img)
        {
            var prevfamilyid = img.FamilyId;
            if (prevfamilyid != 0) {
                img.SetFamilyId(0);
                var size = GetFamilySize(prevfamilyid);
                if (size == 1) {
                    var orphan = _imgList.Values.First(e => e.FamilyId == prevfamilyid);
                    orphan.SetFamilyId(0);
                }
            }
        }

        public static void Split(Img imgX, Img imgY)
        {
            ResetFamily(imgX);
            ResetFamily(imgY);
        }

        public static void RenameFamily(int oldid, int newid)
        {
            var scope = _imgList.Values.Where(e => e.FamilyId == oldid);
            foreach (var img in scope) {
                img.SetFamilyId(newid);
            }
        }

        public static void Combine(Img imgX, Img imgY)
        {
            if (imgX.FamilyId == 0 && imgY.FamilyId == 0) {
                var familyid = AppVars.AllocateFamilyId();
                imgX.SetFamilyId(familyid);
                imgY.SetFamilyId(familyid);
            }
            else {
                if (imgX.FamilyId != 0 && imgY.FamilyId == 0) {
                    imgY.SetFamilyId(imgX.FamilyId);
                }
                else {
                    if (imgX.FamilyId == 0 && imgY.FamilyId != 0) {
                        imgX.SetFamilyId(imgY.FamilyId);
                    }
                    else {
                        if (imgX.FamilyId < imgY.FamilyId) {
                            RenameFamily(imgY.FamilyId, imgX.FamilyId);
                        }
                        else {
                            if (imgX.FamilyId > imgY.FamilyId) {
                                RenameFamily(imgX.FamilyId, imgY.FamilyId);
                            }
                        }
                    }
                }
            }
        }

        public static void SetFamily(Img imgX, int familyid)
        {
            if (imgX.FamilyId == 0) {
                imgX.SetFamilyId(familyid);
            }
            else {
                RenameFamily(imgX.FamilyId, familyid);
            }
        }
    }
}
