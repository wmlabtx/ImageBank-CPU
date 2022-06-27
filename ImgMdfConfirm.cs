namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            imgX.SetLastView(System.DateTime.Now);
            _lastviewed.Add(imgX.GetPalette());
            while (_lastviewed.Count > SIMMAX) {
                _lastviewed.RemoveAt(0);
            }
        }
    }
}