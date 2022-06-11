namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm()
        {
            var imgX = AppVars.ImgPanel[0].Img;
            var imgY = AppVars.ImgPanel[1].Img;

            imgX.AddToHistory(imgY.Id);
            imgX.SetLastView();

            _lastviewed.Add(imgX.GetPalette());
            while (_lastviewed.Count > SIMMAX) {
                _lastviewed.RemoveAt(0);
            }
        }
    }
}