namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm()
        {
            AppVars.ImgPanel[0].Img.IncreaseCounter();
            UpdateLastView(1);
            UpdateLastView(0);
        }

        public static void UpdateLastView(int index)
        {
            AppVars.ImgPanel[index].Img.SetLastView();
        }
    }
}