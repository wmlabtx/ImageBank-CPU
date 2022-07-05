using System;
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

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            WindowLoaded();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            OnStateChanged();
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

        private void ImportClick(object sender, RoutedEventArgs e)
        {
            ImportClick(1000000);
        }

        private void Rotate270Click(object sender, RoutedEventArgs e)
        {
            Rotate270Click();
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            ExportClick();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshClick();
        }

        private void DescreaseRatingClick(object sender, RoutedEventArgs e)
        {
            DescreaseRatingClick();
        }

        private void MoveBackwardClick(object sender, RoutedEventArgs e)
        {
            MoveBackwardClick();
        }
    }
}
