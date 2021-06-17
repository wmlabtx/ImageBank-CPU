using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

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
            await Task.Run(() => { ImgMdf.LoadImgs(AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(null, null, AppVars.Progress); }).ConfigureAwait(true);
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
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _notifyIcon.Visible = true;
            }
        }

        private void WindowSizeChanged()
        {
            RedrawCanvas();
        }

        private void ImportClick()
        {
            Import();
        }

        private async void Import()
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.Import(AppConsts.MaxAdd); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private async void DeleteFolderClick()
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.DeleteFolder(AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
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
            await Task.Run(() => { ImgMdf.UpdateCounter(0); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.UpdateLastView(0); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private static void ButtonRightNextMouseClick()
        {
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
            foreach (System.Windows.Controls.MenuItem item in Menu.Items)
            {
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
            for (var index = 0; index < 2; index++) {
                var filename = AppVars.ImgPanel[index].Img.FileName;
                pBoxes[index].Tag = filename;
                pLabels[index].Tag = filename;

                pBoxes[index].Source = Helper.ImageSourceFromBitmap(AppVars.ImgPanel[index].Bitmap);

                var sb = new StringBuilder();
                sb.Append(filename);

                if (AppVars.ImgPanel[index].Img.Counter > 0) {
                    sb.Append($" ({AppVars.ImgPanel[index].Img.Counter})");
                }

                sb.AppendLine();

                sb.Append($"{Helper.SizeToString(AppVars.ImgPanel[index].Img.Size)} ");
                sb.Append($" ({ AppVars.ImgPanel[index].Bitmap.Width}x{AppVars.ImgPanel[index].Bitmap.Height})");
                sb.AppendLine();

                sb.Append($"{Helper.TimeIntervalToString(DateTime.Now.Subtract(AppVars.ImgPanel[index].Img.LastView))} ago ");
                sb.Append($" [{Helper.TimeIntervalToString(DateTime.Now.Subtract(AppVars.ImgPanel[index].Img.LastChanged))} ago]");

                pLabels[index].Text = sb.ToString();
                var scb = System.Windows.Media.Brushes.White;

                if (index == 1) {
                    if (AppVars.ImgPanel[0].Img.FileName.Equals(AppVars.ImgPanel[1].Img.FileName, StringComparison.OrdinalIgnoreCase)) {
                        pLabels[0].Background = System.Windows.Media.Brushes.LightGray;
                        pLabels[1].Background = System.Windows.Media.Brushes.LightGray;
                    }
                }

                if (AppVars.ImgPanel[index].Img.Counter > 0) {
                    scb = System.Windows.Media.Brushes.Bisque;
                }

                if (AppVars.ImgPanel[index].Bitmap.Width == 2160 ||
                    AppVars.ImgPanel[index].Bitmap.Height == 2160 ||
                    AppVars.ImgPanel[index].Bitmap.Width == 2180 ||
                    AppVars.ImgPanel[index].Bitmap.Height == 2180 ||
                    AppVars.ImgPanel[index].Bitmap.Width == 2880 ||
                    AppVars.ImgPanel[index].Bitmap.Height == 2880 ||
                    AppVars.ImgPanel[index].Bitmap.Width == 3110 ||
                    AppVars.ImgPanel[index].Bitmap.Height == 3110) {
                    scb = System.Windows.Media.Brushes.Pink;
                }

                if (AppVars.ImgPanel[index].Img.FileName.StartsWith(AppConsts.PathMz, StringComparison.OrdinalIgnoreCase)) {
                    scb = System.Windows.Media.Brushes.LightGreen;
                }

                pLabels[index].Background = scb;
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
            await Task.Run(() => { ImgMdf.Delete(AppVars.ImgPanel[index].Img.FileName); }).ConfigureAwait(true);
            if (index == 1) {
                await Task.Run(() => { ImgMdf.UpdateLastView(0); }).ConfigureAwait(true);
            }

            await Task.Run(() => { ImgMdf.Find(AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private async void Rotate(RotateFlipType rft)
        {
            DisableElements();
            await Task.Run(() => { ImgMdf.Rotate(AppVars.ImgPanel[0].Img.FileName, rft); }).ConfigureAwait(true);
            await Task.Run(() => { ImgMdf.Find(AppVars.Progress); }).ConfigureAwait(true);
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

        private static Task ClusteringClickAsync()
        {
            return null;

            /*
            DisableElements();
            await Task.Run(() => { ImgMdf.Clustering(AppConsts.PathHp, 1000); }).ConfigureAwait(true);
            EnableElements();
            */
        }

        private void WindowClosing()
        {
            DisableElements();
            _backgroundWorker?.CancelAsync();
            EnableElements();
        }

        private void DoComputeProgress(object sender, ProgressChangedEventArgs e)
        {
            BackgroundStatus.Text = (string)e.UserState;
        }

        private void DoCompute(object s, DoWorkEventArgs args)
        {
            while (!_backgroundWorker.CancellationPending) {
                ImgMdf.Compute(_backgroundWorker);
                Thread.Sleep(100);
            }

            args.Cancel = true;
        }

        ~MainWindow()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            _backgroundWorker?.Dispose();
            _notifyIcon?.Dispose();
        }
    }
}
