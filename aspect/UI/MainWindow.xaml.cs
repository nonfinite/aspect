using System.Windows;
using System.Windows.Input;

using MahApps.Metro.Controls;

namespace Aspect.UI
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel => mViewModel;

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

        private void _HandleBrowseBack(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.NavBack();
        }

        private void _HandleBrowseForward(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.NavForward();
        }
    }
}
