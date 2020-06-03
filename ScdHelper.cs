using System.Diagnostics.Contracts;
using System.Drawing;

namespace ImageBank
{
    public static class ScdHelper
    {
        public static Scd Compute(Bitmap bitmap)
        {
            Contract.Requires(bitmap != null);

            var descriptor = new SCDDescriptor();
            descriptor.Apply(bitmap, 256, 0);
            return new Scd(descriptor.haarTransformedHistogram);
        }
    }
}