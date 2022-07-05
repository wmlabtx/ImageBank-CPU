namespace ImageBank
{
    public static partial class ImgMdf
    {
        public static void Confirm(int index)
        {
            var imgX = AppVars.ImgPanel[0].Img;
            imgX.SetLastView(System.DateTime.Now);
            _lastviewed.Add(imgX.GetPalette());
            while (_lastviewed.Count > SIMMAX) {
                _lastviewed.RemoveAt(0);
            }

            var imgY = AppVars.ImgPanel[1].Img;
            if (index == 0) {
                imgX.AddRank(imgY.Id, 1);
                imgY.AddRank(imgX.Id, 0);
            }
            else {
                imgX.AddRank(imgY.Id, 0);
                imgY.AddRank(imgX.Id, 1);
            }
        }
    }
}