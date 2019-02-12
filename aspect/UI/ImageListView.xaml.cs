using System.Windows.Controls;

namespace Aspect.UI
{
    public partial class ImageListView : UserControl
    {
        public ImageListView()
        {
            InitializeComponent();
        }

        public bool FocusSearchBox() => mSearchTextBox.Focus();
    }
}
