using System.Linq;

namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static int GetAvailableSceneId()
        {
            var scenes = _imgList.Where(e => e.Value.SceneId != 0).Select(e => e.Value.SceneId).Distinct().OrderBy(e => e).ToArray();
            if (scenes.Length == 0 || scenes[0] != 1) {
                return 1;
            }

            if (scenes.Length == 1) {
                return 2;
            }

            var index = 1;
            while (index < scenes.Length) {
                if (scenes[index] - 1 > scenes[index - 1]) {
                    return scenes[index - 1] + 1;
                }

                index++;
            }

            return scenes[scenes.Length - 1] + 1;
        }

        public static int GetSizeScene(int sceneid)
        {
            if (sceneid == 0) {
                return 0;
            }

            var size = _imgList.Count(e => e.Value.SceneId == sceneid);
            return size;
        }

        public static void Combine()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            var imgY = AppVars.ImgPanel[1].Img;
            if (imgX.SceneId == 0 && imgY.SceneId == 0) {
                var sceneid = GetAvailableSceneId();
                imgX.SetSceneId(sceneid);
                imgY.SetSceneId(sceneid);
                return;
            }

            if (imgX.SceneId == 0 && imgY.SceneId != 0) {
                imgX.SetSceneId(imgY.SceneId);
                return;
            }

            if (imgX.SceneId != 0 && imgY.SceneId == 0) {
                imgY.SetSceneId(imgX.SceneId);
                return;
            }

            if (imgX.SceneId != 0 && imgY.SceneId != 0 && imgX.SceneId != imgY.SceneId) {
                if (imgX.SceneId < imgY.SceneId) {
                    imgY.SetSceneId(imgX.SceneId);
                }
                else {
                    imgX.SetSceneId(imgY.SceneId);
                }

                return;
            }
        }

        public static void Detach(Img img)
        {
            if (img.SceneId == 0) {
                return;
            }

            var size = GetSizeScene(img.SceneId);
            if (size == 1) {
                return;
            }

            var sceneid = GetAvailableSceneId();
            img.SetSceneId(sceneid);
        }

        public static void DetachLeft()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            Detach(imgX);
        }

        public static void DetachRight()
        {
            var imgY = AppVars.ImgPanel[1].Img;
            Detach(imgY);
        }
    }
}
