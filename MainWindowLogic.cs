using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace ImageBank
{
    public partial class MainWindow
    {
        private double _picsMaxWidth;
        private double _picsMaxHeight;
        private double _labelMaxHeight;
        private BackgroundWorker _backgroundWorker;
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
            AppVars.BackgroundProgress = new Progress<string>(message => BackgroundStatus.Text = message);

            DisableElements();
            await Task.Run(() => { AppVars.Collection.Load(AppVars.Progress); }).ConfigureAwait(true);
            await Task.Run(() => { AppVars.Collection.Find(0, 0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            
            EnableElements();

            AppVars.SuspendEvent = new ManualResetEvent(true);

            _backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true, WorkerReportsProgress = true };
            _backgroundWorker.DoWork += DoComputeSim;
            _backgroundWorker.ProgressChanged += DoComputeSimProgress;
            _backgroundWorker.RunWorkerAsync();
        }

        private void WindowClosing()
        {
            DisableElements();
            _backgroundWorker?.CancelAsync();
            EnableElements();
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

        private async void ImportClick()
        {
            
            await Task.Run(() => { AppVars.Collection.Import(AppVars.BackgroundProgress); }).ConfigureAwait(true);

            DisableElements();
            await Task.Run(() => { AppVars.Collection.Find(0, 0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private async void FamilyCombineClick()
        {
            if (AppVars.ImgPanel[0].Family > 0 && AppVars.ImgPanel[0].Family == AppVars.ImgPanel[1].Family) {
                return;
            }

            DisableElements();
            await Task.Run(() => { AppVars.Collection.FamilyCombine(AppVars.ImgPanel[0].Id, AppVars.ImgPanel[1].Id); }).ConfigureAwait(true);
            await Task.Run(() => { AppVars.Collection.Find(AppVars.ImgPanel[0].Id, AppVars.ImgPanel[1].Id, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private async void FamilyBreakClick()
        {
            if (AppVars.ImgPanel[0].Family <= 0 || AppVars.ImgPanel[0].Family != AppVars.ImgPanel[1].Family) {
                return;
            }

            DisableElements();
            await Task.Run(() => { AppVars.Collection.FamilyBreak(AppVars.ImgPanel[0].Id, AppVars.ImgPanel[1].Id); }).ConfigureAwait(true);
            await Task.Run(() => { AppVars.Collection.Find(AppVars.ImgPanel[0].Id, AppVars.ImgPanel[1].Id, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private void PictureLeftBoxMouseClick()
        {
            ImgPanelDelete(0);
            AppVars.Collection.UpdateLastView(AppVars.ImgPanel[1].Id);
        }

        private void PictureRightBoxMouseClick()
        {
            ImgPanelDelete(1);
            AppVars.Collection.UpdateLastView(AppVars.ImgPanel[0].Id);
        }

        private async void ButtonLeftNextMouseClick()
        {
            AppVars.Collection.UpdateHistory(AppVars.ImgPanel[0].Id, AppVars.ImgPanel[1].Id);
            AppVars.Collection.UpdateHistory(AppVars.ImgPanel[1].Id, AppVars.ImgPanel[0].Id);
            AppVars.Collection.UpdateLastView(AppVars.ImgPanel[0].Id);
            AppVars.Collection.UpdateLastView(AppVars.ImgPanel[1].Id);

            DisableElements();
            await Task.Run(() => { AppVars.Collection.Find(0, 0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }

        private void ButtonRightNextMouseClick()
        {
        }

        private void DoComputeSimProgress(object sender, ProgressChangedEventArgs e)
        {
            BackgroundStatus.Text = (string)e.UserState;
        }

        private void DoComputeSim(object s, DoWorkEventArgs args)
        {
            while (!_backgroundWorker.CancellationPending) {
                AppVars.Collection.Compute(_backgroundWorker);
                Thread.Sleep(200);
            }

            args.Cancel = true;
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
            if (AppVars.ImgPanel[0] == null || AppVars.ImgPanel[1] == null)
            {
                return;
            }

            var pBoxes = new[] { BoxLeft, BoxRight };
            var pLabels = new[] { LabelLeft, LabelRight };
            for (var index = 0; index < 2; index++) {
                var id = AppVars.ImgPanel[index].Id;
                pBoxes[index].Tag = id;
                pLabels[index].Tag = id;

                pBoxes[index].Source = Helper.ImageSourceFromBitmap(AppVars.ImgPanel[index].Bitmap);

                var sb = new StringBuilder();
                sb.Append($"{AppVars.ImgPanel[index].Name}");

                if (AppVars.ImgPanel[index].Family > 0) {
                    sb.Append($" [{AppVars.ImgPanel[index].Family}:{AppVars.ImgPanel[index].FamilySize}]");
                }

                if (AppVars.ImgPanel[index].Generation > 0) {
                    sb.Append($" {AppVars.ImgPanel[index].Generation}");
                }

                sb.AppendLine();
                
                sb.Append($"{Helper.SizeToString(AppVars.ImgPanel[index].Length)} ");
                sb.Append($" ({ AppVars.ImgPanel[index].Bitmap.Width}x{AppVars.ImgPanel[index].Bitmap.Height})");
                sb.AppendLine();

                sb.Append($"{Helper.TimeIntervalToString(DateTime.Now.Subtract(AppVars.ImgPanel[index].LastView))} ago");
                sb.Append($" ({AppVars.ImgPanel[index].Distance:F2})");
                sb.Append($" {AppVars.ImgPanel[index].Done:F1}%");

                pLabels[index].Text = sb.ToString();
                var scb = System.Windows.Media.Brushes.Bisque;
                if (AppVars.ImgPanel[index].Family > 0) {
                    scb = System.Windows.Media.Brushes.GreenYellow;
                }
                else {
                    if (AppVars.ImgPanel[index].Generation == 0) {
                        scb = System.Windows.Media.Brushes.White;
                    }
                }

                pLabels[index].Background = scb;
            }

            if (AppVars.ImgPanel[0].Family > 0 && AppVars.ImgPanel[0].Family == AppVars.ImgPanel[1].Family) {
                pLabels[0].Background = System.Windows.Media.Brushes.LightGreen;
                pLabels[1].Background = System.Windows.Media.Brushes.LightGreen;
            }

            if (AppVars.ImgPanel[0].Id == AppVars.ImgPanel[1].Id) {
                pLabels[0].Background = System.Windows.Media.Brushes.LightGray;
                pLabels[1].Background = System.Windows.Media.Brushes.LightGray;
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
            if (a > 1.0)
            {
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
            await Task.Run(() => { AppVars.Collection.Delete(AppVars.ImgPanel[index].Id); }).ConfigureAwait(true);
            await Task.Run(() => { AppVars.Collection.Find(0, 0, AppVars.Progress); }).ConfigureAwait(true);
            DrawCanvas();
            EnableElements();
        }
    }
}
