using System.Drawing;

namespace ImageBank
{
    public interface Cfilter
    {
        Bitmap Apply(Bitmap img);
    }
}
