using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Aspect.UI
{
    public partial class ImageTagView : UserControl
    {
        public ImageTagView()
        {
            InitializeComponent();
        }

        public ImageTagViewModel ViewModel => DataContext as ImageTagViewModel;

        private void _HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                _Refresh();
            }
        }

        private async void _HandleTagTextKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = (TextBox) sender;
                var tag = textBox.Text;
                var vm = ViewModel;
                if (vm != null)
                {
                    await vm.AddTag(tag);
                    textBox.Clear();
                }
            }
        }

        private void _HandleVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                _Refresh();
            }
        }

        private void _Refresh()
        {
            ViewModel?.Refresh();
            Dispatcher.Invoke(mAddBox.Focus, DispatcherPriority.Input);
        }
    }
}
