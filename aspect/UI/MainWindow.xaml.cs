using System.Windows;

using MahApps.Metro.Controls;

namespace Aspect.UI
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

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
