using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ImageBank
{
    public sealed partial class MainWindow : IDisposable
    {
        private double _picsMaxWidth;
        private double _picsMaxHeight;
        private double _labelMaxHeight;

        private readonly NotifyIcon _notifyIcon = new NotifyIcon();
        private BackgroundWorker _backgroundWorker;

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

            DisableElements();
            await Task.Run(() => { ImgMdf.LoadImages(AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            
            EnableElements();

            AppVars.SuspendEvent = new ManualResetEvent(true);

            _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = true };
            _backgroundWorker.DoWork += DoCompute;
            _backgroundWorker.ProgressChanged += DoComputeProgress;
            _backgroundWorker.RunWorkerAsync();
        }

        private void OnStateChanged()
        {
            if (WindowState == WindowState.Minimized) {
                Hide();
                _notifyIcon.Visible = true;
            }
        }

        private static void ImportClick()
        {
            AppVars.ImportMode = true;
        }

        private async void ExportClick()
        {
            await Task.Run(() => { ImgMdf.Export(AppVars.ImgPanel[0].Img, AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Export(AppVars.ImgPanel[1].Img, AppVars.Progress); }).ConfigureAwait(true);
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
            if (AppVars.ImgPanel[0] == null || AppVars.ImgPanel[1] == null) {
                return;
            }

            var pBoxes = new[] { BoxLeft, BoxRight };
            var pLabels = new[] { LabelLeft, LabelRight };
            var fs = new float[2];
            for (var index = 0; index < 2; index++) {
                pBoxes[index].Source = BitmapHelper.ImageSourceFromBitmap(AppVars.ImgPanel[index].Bitmap);

                var sb = new StringBuilder();
                sb.Append($"{AppVars.ImgPanel[index].Img.Name} [{AppVars.ImgPanel[index].Img.Id}]");

                var historysize = AppVars.ImgPanel[index].Img.GetHistorySize();
                if (historysize != 0) {
                    sb.Append($" H{historysize}");
                }

                if (AppVars.ImgPanel[index].Img.FamilyId > 0) {
                    var familysize = AppImgs.GetFamilySize(AppVars.ImgPanel[index].Img.FamilyId);
                    sb.Append($" [{AppVars.ImgPanel[index].Img.FamilyId}:{familysize}]");
                }

                sb.AppendLine();

                sb.Append($"{Helper.SizeToString(AppVars.ImgPanel[index].Size)} ");
                sb.Append($" ({ AppVars.ImgPanel[index].Bitmap.Width}x{AppVars.ImgPanel[index].Bitmap.Height})");
                sb.AppendLine();

                if (AppVars.ImgPanel[index].Img.Year != 0) {
                    sb.Append($"[{AppVars.ImgPanel[index].Img.Year}]");
                }

                sb.Append($" {Helper.TimeIntervalToString(DateTime.Now.Subtract(AppVars.ImgPanel[index].Img.LastView))} ago ");

                fs[index] = AppVars.ImgPanel[index].Size / ((float)AppVars.ImgPanel[index].Bitmap.Width * AppVars.ImgPanel[index].Bitmap.Height);
                sb.Append($" [{fs[index]:F4}]");

                pLabels[index].Text = sb.ToString();

                /*
                if (index == 1) {
                    if (fs[0] < fs[1]) {
                        pLabels[0].Background = System.Windows.Media.Brushes.LightSalmon;
                        pLabels[1].Background = System.Windows.Media.Brushes.White;
                    }
                    else {
                        pLabels[0].Background = System.Windows.Media.Brushes.White;
                        pLabels[1].Background = System.Windows.Media.Brushes.LightSalmon;
                    }
                }
                else {
                    pLabels[index].Background = System.Windows.Media.Brushes.White;
                }
                */

                if (AppVars.ImgPanel[index].Img.FamilyId > 0) {
                    pLabels[index].Background = Helper.GetBrush(AppVars.ImgPanel[index].Img.FamilyId);
                }
                else {
                    pLabels[index].Background = System.Windows.Media.Brushes.White;
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
                ws[index] = _picsMaxWidth / 2;
                hs[index] = _picsMaxHeight;
                if (AppVars.ImgPanel[index] != null && AppVars.ImgPanel[index].Bitmap != null)
                {
                    ws[index] = AppVars.ImgPanel[index].Bitmap.Width;
                    hs[index] = AppVars.ImgPanel[index].Bitmap.Height;
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

        private async void ImgPanelDelete(int index)
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.Delete(AppVars.ImgPanel[index].Img.Id); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private async void Rotate(RotateFlipType rft)
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.Rotate(AppVars.ImgPanel[0].Img.Id, rft, AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(AppVars.ImgPanel[0].Img.Id, AppVars.Progress); }).ConfigureAwait(true);
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
            _backgroundWorker?.Dispose();
        }

        private void RefreshClick()
        {
            DisableElements();
            DrawCanvas();
            EnableElements();
        }

        private async void CombineFamilyClick()
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.CombineFamily(); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private async void SplitFamilyClick()
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.SplitFamily(); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private void DoComputeProgress(object sender, ProgressChangedEventArgs e)
        {
            BackgroundStatus.Text = (string)e.UserState;
        }

        private void DoCompute(object s, DoWorkEventArgs args)
        {
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            while (!_backgroundWorker.CancellationPending) {
                ImgMdf.Compute(_backgroundWorker);
                Thread.Sleep(100);
            }

            args.Cancel = true;
        }
    }
}