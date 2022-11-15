using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace ImageBank
{
    public sealed partial class MainWindow : IDisposable
    {
        private double _picsMaxWidth;
        private double _picsMaxHeight;
        private double _labelMaxHeight;

        private readonly NotifyIcon _notifyIcon = new NotifyIcon();

        private async void WindowLoaded()
        {
            BoxLeft.MouseDown += PictureLeftBoxMouseClick;
            BoxRight.MouseDown += PictureRightBoxMouseClick;

            LabelLeft.MouseDown += ButtonLeftNextMouseClick;
            LabelRight.MouseDown += ButtonRightNextMouseClick;

            Left = SystemParameters.WorkArea.Left + AppConsts.WindowMargin;
            Top = SystemParameters.WorkArea.Top + AppConsts.WindowMargin;
            Width = SystemParameters.WorkArea.Width - AppConsts.WindowMargin * 2;
            Height = SystemParameters.WorkArea.Height - AppConsts.WindowMargin * 2;
            Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - AppConsts.WindowMargin - Width) / 2;
            Top = SystemParameters.WorkArea.Top + (SystemParameters.WorkArea.Height - AppConsts.WindowMargin - Height) / 2;

            _picsMaxWidth = Grid.ActualWidth;
            _labelMaxHeight = LabelLeft.ActualHeight;
            _picsMaxHeight = Grid.ActualHeight - _labelMaxHeight;

            _notifyIcon.Icon = new Icon(@"app.ico");
            _notifyIcon.Visible = false;
            _notifyIcon.DoubleClick +=
                delegate
                {
                    Show();
                    WindowState = WindowState.Normal;
                    _notifyIcon.Visible = false;
                    RedrawCanvas();
                };

            AppVars.Progress = new Progress<string>(message => Status.Text = message);

            await Task.Run(() => { ImgMdf.LoadImages(AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
        }

        private void OnStateChanged()
        {
            if (WindowState == WindowState.Minimized) {
                Hide();
                _notifyIcon.Visible = true;
            }
        }

        private async void ImportClick()
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.Import(AppVars.Progress); }).ConfigureAwait(true);
            EnableElements();
        }

        private async void ExportClick()
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.Export(0, AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Export(1, AppVars.Progress); }).ConfigureAwait(true);
            EnableElements();
        }

        private void PictureLeftBoxMouseClick()
        {
            ImgPanelDelete(0);
        }

        private void PictureRightBoxMouseClick()
        {
            ImgPanelDelete(1);
        }

        private async void ButtonLeftNextMouseClick()
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.Confirm(); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private void DisableElements()
        {
            ElementsEnable(false);
        }

        private void EnableElements()
        {
            ElementsEnable(true);
        }

        private void ElementsEnable(bool enabled)
        {
            foreach (System.Windows.Controls.MenuItem item in Menu.Items) {
                item.IsEnabled = enabled;
            }

            Status.IsEnabled = enabled;
            BoxLeft.IsEnabled = enabled;
            BoxRight.IsEnabled = enabled;
            LabelLeft.IsEnabled = enabled;
            LabelRight.IsEnabled = enabled;
        }

        private void DrawCanvas()
        {
            var panels = new ImgPanel[2];
            panels[0] = AppPanels.GetImgPanel(0);
            panels[1] = AppPanels.GetImgPanel(1);
            if (panels[0] == null || panels[1] == null) {
                return;
            }

            var pBoxes = new[] { BoxLeft, BoxRight };
            var pLabels = new[] { LabelLeft, LabelRight };
            var fs = new float[2];
            for (var index = 0; index < 2; index++) {
                pBoxes[index].Source = BitmapHelper.ImageSourceFromBitmap(panels[index].Bitmap);

                var sb = new StringBuilder();
                sb.Append($"{panels[index].Img.Name} [{panels[index].Img.Id}");
                if (panels[index].Img.FamilyId != 0) {
                    sb.Append($":{panels[index].Img.FamilyId}");
                    var familysize = AppImgs.GetFamilySize(panels[index].Img.FamilyId);
                    sb.Append($":{familysize}");
                }

                sb.Append(']');
                sb.AppendLine();

                sb.Append($"{Helper.SizeToString(panels[index].Size)} ");
                sb.Append($" ({panels[index].Bitmap.Width}x{panels[index].Bitmap.Height})");
                sb.AppendLine();

                if (panels[index].Img.Year != 0) {
                    sb.Append($"[{panels[index].Img.Year}]");
                }

                sb.Append($" {Helper.TimeIntervalToString(DateTime.Now.Subtract(panels[index].Img.LastView))} ago ");

                fs[index] = panels[index].Size / ((float)panels[index].Bitmap.Width * panels[index].Bitmap.Height);
                sb.Append($" [{fs[index]:F4}]");

                pLabels[index].Text = sb.ToString();

                if (index == 1) {
                    if (fs[0] < fs[1]) {
                        pLabels[0].Background = System.Windows.Media.Brushes.LightSalmon;
                        pLabels[1].Background = System.Windows.Media.Brushes.White;
                    }
                    else {
                        pLabels[0].Background = System.Windows.Media.Brushes.White;
                        pLabels[1].Background = System.Windows.Media.Brushes.LightSalmon;
                    }

                    if (panels[0].Img.FamilyId != 0) {
                        if (panels[0].Img.FamilyId == panels[1].Img.FamilyId) {
                            pLabels[0].Background = System.Windows.Media.Brushes.LightGreen;
                            pLabels[1].Background = System.Windows.Media.Brushes.LightGreen;
                        }
                        else {
                            pLabels[0].Background = System.Windows.Media.Brushes.Yellow;
                        }
                    }
                    else {
                        if (panels[1].Img.FamilyId != 0 && panels[1].Img.FamilyId != panels[0].Img.FamilyId) {
                            pLabels[1].Background = System.Windows.Media.Brushes.YellowGreen;
                        }
                    }
                }
            }

            RedrawCanvas();
        }

        private void RedrawCanvas()
        {
            var ws = new double[2];
            var hs = new double[2];
            for (var index = 0; index < 2; index++)
            {
                var panel = AppPanels.GetImgPanel(index);
                ws[index] = _picsMaxWidth / 2;
                hs[index] = _picsMaxHeight;
                if (panel != null && panel.Bitmap != null)
                {
                    ws[index] = panel.Bitmap.Width;
                    hs[index] = panel.Bitmap.Height;
                }
            }

            var aW = _picsMaxWidth / (ws[0] + ws[1]);
            var aH = _picsMaxHeight / Math.Max(hs[0], hs[1]);
            var a = Math.Min(aW, aH);
            if (a > 1.0) {
                a = 1.0;
            }

            SizeToContent = SizeToContent.Manual;
            Grid.ColumnDefinitions[0].Width = new GridLength(ws[0] * a, GridUnitType.Pixel);
            Grid.ColumnDefinitions[1].Width = new GridLength(ws[1] * a, GridUnitType.Pixel);
            Grid.RowDefinitions[0].Height = new GridLength(Math.Max(hs[0], hs[1]) * a, GridUnitType.Pixel);
            Grid.Width = (ws[0] + ws[1]) * a;
            Grid.Height = Math.Max(hs[0], hs[1]) * a + _labelMaxHeight;
            SizeToContent = SizeToContent.WidthAndHeight;
            Left = SystemParameters.WorkArea.Left + (SystemParameters.WorkArea.Width - AppConsts.WindowMargin - Width) / 2;
            Top = SystemParameters.WorkArea.Top + (SystemParameters.WorkArea.Height - AppConsts.WindowMargin - Height) / 2;
        }

        private async void ImgPanelDelete(int idpanel)
        {
            DisableElements();
            var id = AppPanels.GetImgPanel(idpanel).Img.Id;
            await Task.Run(() => { ImgMdf.Delete(id); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private async void Rotate(RotateFlipType rft)
        {
            DisableElements();
            var id = AppPanels.GetImgPanel(0).Img.Id;
            await Task.Run(() => { ImgMdf.Rotate(id, rft, AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(id, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private void Rotate90Click()
        {
            Rotate(RotateFlipType.Rotate90FlipNone);
        }

        private void Rotate180Click()
        {
            Rotate(RotateFlipType.Rotate180FlipNone);
        }

        private void Rotate270Click()
        {
            Rotate(RotateFlipType.Rotate270FlipNone);
        }

        ~MainWindow()
        {
            Dispose();
        }

        public void Dispose()
        {
            ClassDispose();
            GC.SuppressFinalize(this);
        }

        private void ClassDispose()
        {
            _notifyIcon?.Dispose();
        }

        private void RefreshClick()
        {
            DrawCanvas();
        }

        private void LeftMoveClick()
        {
            AppPanels.MoveLeftPosition(AppVars.Progress);
            DrawCanvas();
        }

        private void RightMoveClick()
        {
            AppPanels.MoveRightPosition(AppVars.Progress);
            DrawCanvas();
        }

        private void FirstMoveClick()
        {
            AppPanels.SetFirstPosition(AppVars.Progress);
            DrawCanvas();
        }

        private void LastMoveClick()
        {
            AppPanels.SetLastPosition(AppVars.Progress);
            DrawCanvas();
        }

        private void CombineClick()
        {
            var imgX = AppPanels.GetImgPanel(0).Img;
            var imgY = AppPanels.GetImgPanel(1).Img;
            AppImgs.Combine(imgX, imgY);
            DrawCanvas();
        }

        private void SplitClick()
        {
            var imgX = AppPanels.GetImgPanel(0).Img;
            var imgY = AppPanels.GetImgPanel(1).Img;
            AppImgs.Split(imgX, imgY);
            DrawCanvas();
        }

        private void OnKeyDown(Key key)
        {
            switch (key) {
                case Key.E:
                    RightMoveClick();
                    break;
                case Key.Q:
                    LeftMoveClick();
                    break;
                case Key.C:
                    CombineClick();
                    break;
            }
        }
    }
}