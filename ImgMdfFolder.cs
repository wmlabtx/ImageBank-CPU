using System.Linq;

namespace ImageBank
{
    public partial class ImgMdf
    {
        public int FolderSize(int folder)
        {
            lock (_imglock) {
                var foldersize = _imgList.Count(e => folder == e.Value.Folder);

                return foldersize;
            }
        }
    }
}
