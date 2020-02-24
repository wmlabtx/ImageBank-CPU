using System.IO;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public void MoveTo(int id, string path)
        {
            if (_imgList.TryGetValue(id, out var imgX)) {
                var oldfile = imgX.File;
                imgX.Path = path;
                var newfile = imgX.File;
                File.Move(oldfile, newfile);
            }
        }
    }
}
