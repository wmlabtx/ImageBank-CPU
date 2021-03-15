using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public static int FolderSize(string folder)
        {
            lock (_imglock) {
                var foldersize = _imgList.Count(e => folder.Equals(e.Value.Folder));
                return foldersize;
            }
        }
    }
}
