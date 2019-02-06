using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using Aspect.Properties;

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
