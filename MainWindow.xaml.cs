using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
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

        private void ImportClick(object sender, RoutedEventArgs e)
        {
            ImportClick(1000000);
        }

        private void Rotate270Click(object sender, RoutedEventArgs e)
        {
            Rotate270Click();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z) {
                ButtonLeftNextMouseClick();
            }
        }

        private void ExportClick(object sender, RoutedEventArgs e)
        {
            ExportClick();
        }

        private void CombineClick(object sender, RoutedEventArgs e)
        {
            CombineClick();
        }

        private void DetachLeftClick(object sender, RoutedEventArgs e)
        {
            DetachLeftClick();
        }

        private void DetachRightClick(object sender, RoutedEventArgs e)
        {
            DetachRightClick();
        }
    }
}
