using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public int GetPersonSize(string person)
        {
            lock (_imglock) {
                return _imgList.Count(e => e.Value.Person.Equals(person, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void AssingPerson(int id, string person)
        {
            lock (_imglock) {
                if (!_imgList.TryGetValue(id, out Img img)) {
                    return;
                }

                if (img.Person.Equals(person, StringComparison.OrdinalIgnoreCase)) {
                    return;
                }

                img.Person = person;
                FindNext(img.Id, out var nextid, out var sim);
                img.NextId = nextid;
                img.Sim = sim;

                AppVars.ImgPanel[0] = GetImgPanel(img.Id);
                AppVars.ImgPanel[1] = GetImgPanel(nextid);
            }
        }
    }
}
