using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImageBank
{
    public partial class MainWindow : Window
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

        private void ImportClick(object sender, EventArgs e)
        {
            ImportClick();
        }

        private void ExitClick(object sender, EventArgs e)
        {
            Close();
        }

        private void MoveToTheRightClick(object sender, RoutedEventArgs e)
        {
            MoveToTheRight();
        }

        private void MoveToClick(object sender, RoutedEventArgs e)
        {
            var tag = (string)((MenuItem)sender).Tag;
            MoveTo(tag);
        }
    }
}
