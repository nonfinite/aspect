using System.Windows;
using System.Windows.Controls;

namespace Aspect.UI
{
    public partial class ImageListView : UserControl
    {
        public ImageListView()
        {
            InitializeComponent();
        }

        private void _CopySelectedFiles(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.CopySelectedFiles();
            }
        }

        public bool FocusSearchBox() => mSearchTextBox.Focus();
    }
}
