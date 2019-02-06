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

        private void _HandleImageListVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var imageList = (ListView) sender;

            var item = imageList.SelectedItem;
            if (item != null)
            {
                imageList.ScrollIntoView(item);
            }
        }

        public bool FocusSearchBox() => mSearchTextBox.Focus();
    }
}
