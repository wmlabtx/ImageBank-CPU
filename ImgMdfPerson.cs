using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public int GetPersonSize(int person)
        {
            lock (_imglock) {
                return _imgList.Count(e => e.Value.Person == person);
            }
        }

        public void AssingPerson(int id, int person)
        {
            lock (_imglock) {
                if (!_imgList.TryGetValue(id, out Img img)) {
                    return;
                }

                if (img.Person == person) {
                    return;
                }

                var imgdata = Helper.ReadData(img.FileName);
                if (imgdata == null) {
                    return;
                }

                var newid = AllocateId();
                var newimg = new Img(
                    id: newid,
                    checksum: img.Checksum,
                    person: person,
                    lastview: img.LastView,
                    nextid: newid,
                    distance: 256f,
                    lastid: 0,
                    vector: img.Vector(),
                    format: img.Format,
                    scalar: img.Scalar(),
                    counter: 0);

                Delete(id);
                Add(newimg);
                Helper.WriteData(newimg.FileName, imgdata);
                Helper.DeleteToRecycleBin(img.FileName);

                FindNext(newid, out var lastid, out var nextid, out var distance);
                newimg.LastId = lastid;
                newimg.NextId = nextid;
                newimg.Distance = distance;

                AppVars.ImgPanel[0] = GetImgPanel(newid);
                AppVars.ImgPanel[1] = GetImgPanel(nextid);
            }
        }
    }
}
