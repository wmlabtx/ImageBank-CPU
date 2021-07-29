using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageBank
{
    public sealed partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Trace.Listeners.Add(new ConsoleTraceListener());
        }

        private MenuItem GetMenuItem(int id)
        {
            var mi = new MenuItem() { Header = $"{ImgMdf.Family[id]} [Id:{id}]", Tag = id };
            mi.Click += SetFamilyClick;
            return mi;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            WindowLoaded();

            Family.Items.Clear();
            Family.Items.Add(GetMenuItem(0));
            Family.Items.Add(new Separator());

            var mia = new MenuItem() { Header = "Adults" };
            Family.Items.Add(mia);
            mia.Items.Add(GetMenuItem(15));
            mia.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));

            var mic = new MenuItem() { Header = "Classic" };
            Family.Items.Add(mic);
            mic.Items.Add(GetMenuItem(9));
            mic.Items.Add(GetMenuItem(10));
            mic.Items.Add(GetMenuItem(14));
            mic.Items.Add(GetMenuItem(26));
            mic.Items.Add(GetMenuItem(27));
            mic.Items.Add(GetMenuItem(28));
            mic.Items.Add(GetMenuItem(33));
            mic.Items.Add(GetMenuItem(34));
            mic.Items.Add(GetMenuItem(35));
            mic.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));

            var mid = new MenuItem() { Header = "Drawings" };
            Family.Items.Add(mid);
            mid.Items.Add(GetMenuItem(30));
            mid.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));

            var mih = new MenuItem() { Header = "Hardcore" };
            Family.Items.Add(mih);
            mih.Items.Add(GetMenuItem(21));
            mih.Items.Add(GetMenuItem(22));
            mih.Items.Add(GetMenuItem(23));
            mih.Items.Add(GetMenuItem(24));
            mih.Items.Add(GetMenuItem(25));
            mih.Items.Add(GetMenuItem(29));
            mih.Items.Add(GetMenuItem(32));
            mih.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));

            var mij = new MenuItem() { Header = "Japan" };
            Family.Items.Add(mij);
            mij.Items.Add(GetMenuItem(16));
            mij.Items.Add(GetMenuItem(36));
            mij.Items.Add(GetMenuItem(37));
            mij.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));

            var mim = new MenuItem() { Header = "Models" };
            Family.Items.Add(mim);
            mim.Items.Add(GetMenuItem(12));
            mim.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));

            var min = new MenuItem() { Header = "Nature" };
            Family.Items.Add(min);
            min.Items.Add(GetMenuItem(11));
            min.Items.Add(GetMenuItem(13));
            min.Items.Add(GetMenuItem(31));
            min.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));

            var miv = new MenuItem() { Header = "Vintage" };
            Family.Items.Add(miv);
            miv.Items.Add(GetMenuItem(1));
            miv.Items.Add(GetMenuItem(2));
            miv.Items.Add(GetMenuItem(3));
            miv.Items.Add(GetMenuItem(4));
            miv.Items.Add(GetMenuItem(5));
            miv.Items.Add(GetMenuItem(6));
            miv.Items.Add(GetMenuItem(7));
            miv.Items.Add(GetMenuItem(8));
            miv.Items.Add(GetMenuItem(17));
            miv.Items.Add(GetMenuItem(18));
            miv.Items.Add(GetMenuItem(19));
            miv.Items.Add(GetMenuItem(20));
            miv.Items.SortDescriptions.Add(new SortDescription("Header", ListSortDirection.Ascending));
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            WindowClosing();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            OnStateChanged();
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            WindowSizeChanged();
        }

        private void PictureLeftBoxMouseClick(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                PictureLeftBoxMouseClick();
            }
        }

        private void PictureRightBoxMouseClick(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                PictureRightBoxMouseClick();
            }
        }

        private void ButtonLeftNextMouseClick(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ButtonLeftNextMouseClick();
            }
        }

        private void ButtonRightNextMouseClick(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ButtonRightNextMouseClick();
            }
        }


        private void Rotate90Click(object sender, EventArgs e)
        {
            Rotate90Click();
        }

        private void Rotate180Click(object sender, EventArgs e)
        {
            Rotate180Click();
        }

        private void ExitClick(object sender, EventArgs e)
        {
            Close();
        }

        private static async System.Threading.Tasks.Task ClusteringClickAsync(object sender, RoutedEventArgs e)
        {
            await ClusteringClickAsync();
        }

        private void ImportClick(object sender, RoutedEventArgs e)
        {
            ImportClick();
        }

        private void GetDescriptorsClick(object sender, RoutedEventArgs e)
        {
            GetDescriptorsClick();
        }

        private void SetFamilyClick(object sender, RoutedEventArgs e)
        {
            var family = (int)((MenuItem)e.Source).Tag;
            SetFamily(family);
        }

        private void Rotate270Click(object sender, RoutedEventArgs e)
        {
            Rotate270Click();
        }
    }
}
