using System.Linq;
using System.Text;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void AddFeature(byte feature)
        {
            var array = AppVars.ImgPanel[0].Img.Features.ToList();
            if (array.Contains(feature)) {
                return;
            }

            array.Add(feature);
            AppVars.ImgPanel[0].Img.Features = array.OrderBy(x => x).ToArray();
        }

        public static void RemoveFeature(byte feature)
        {
            var array = AppVars.ImgPanel[0].Img.Features.ToList();
            if (!array.Contains(feature)) {
                return;
            }

            array.Remove(feature);
            AppVars.ImgPanel[0].Img.Features = array.OrderBy(x => x).ToArray();
        }

        public static string GetFeaturesString(byte[] features)
        {
            var sb = new StringBuilder();
            foreach (var feature in features) { 
                var description = FeaturesList[feature];
                sb.Append(description.Substring(0, 2));
            }

            return sb.ToString();
        }
    }
}
