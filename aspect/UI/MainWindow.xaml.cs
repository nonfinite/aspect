using System.Windows;
using System.Windows.Input;

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

        private void _HandleBrowseBack(object sender, ExecutedRoutedEventArgs e) => ViewModel.NavBack();

        private void _HandleBrowseForward(object sender, ExecutedRoutedEventArgs e) => ViewModel.NavForward();

        private void _HandleImagesFlyoutIsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is Flyout flyout) || !flyout.IsOpen)
            {
                return;
            }

            var item = mImageList.SelectedItem;
            if (item != null)
            {
                mImageList.ScrollIntoView(item);
            }
        }
    }
}
