using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using Aspect.Services;

using MahApps.Metro.Controls;

namespace Aspect.UI
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            if (Settings.Default.MaximizeOnStartup)
            {
                WindowState = WindowState.Maximized;
            }
        }

        public MainViewModel ViewModel => mViewModel;

        private void _CloseFlyout(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is Flyout flyout)
            {
                flyout.IsOpen = false;
            }
        }

        private void _HandleBrowseBack(object sender, ExecutedRoutedEventArgs e) => ViewModel.NavBack();

        private void _HandleBrowseForward(object sender, ExecutedRoutedEventArgs e) => ViewModel.NavForward();

        private void _HandleNavigateJournal(object sender, ExecutedRoutedEventArgs e)
        {
            var logView = OwnedWindows.OfType<LogView>().FirstOrDefault();

            if (logView == null)
            {
                logView = new LogView {Owner = this};
                logView.Show();
            }

            if (logView.WindowState == WindowState.Minimized)
            {
                logView.WindowState = WindowState.Normal;
            }

            logView.Activate();
        }

        private async void _HandleOpen(object sender, RoutedEventArgs e) => await ViewModel.Open();

        private void _HandleSearch(object sender, ExecutedRoutedEventArgs e)
        {
            if (mImageListFlyout.IsOpen)
            {
                mImageListView.FocusSearchBox();
            }
            else
            {
                mImageListFlyout.IsOpenChanged += _FocusOnOpen;
                mImageListFlyout.IsOpen = true;
            }

            void _FocusOnOpen(object sender3, RoutedEventArgs e3)
            {
                if (mImageListFlyout.IsOpen)
                {
                    mImageListFlyout.IsOpenChanged -= _FocusOnOpen;
                    Dispatcher.Invoke(mImageListView.FocusSearchBox, DispatcherPriority.Input);
                }
            }
        }
    }
}
