using System;
using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static void UpdateGeneration(int index)
        {
            lock (_imglock) {
                AppVars.ImgPanel[index].Img.Generation += 1;
            }
        }

        public static void UpdateLastView(int index)
        {
            lock (_imglock) {
                var family = AppVars.ImgPanel[index].Img.Family;
                if (family == 0) {
                    AppVars.ImgPanel[index].Img.LastView = DateTime.Now;
                }
                else {
                    var scope = _imgList
                        .Where(e => e.Value.Family != 0 && e.Value.Family == family)
                        .Select(e => e.Value)
                        .ToArray();

                    foreach (var img in scope) {
                        var lvdiff = _random.NextShort(0, 300);
                        img.LastView = DateTime.Now.AddSeconds(-lvdiff);
                    }
                }
            }
        }
    }
}