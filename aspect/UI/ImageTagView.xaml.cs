using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Aspect.UI
{
    public partial class ImageTagView : UserControl
    {
        public ImageTagView()
        {
            InitializeComponent();
        }

        public ImageTagViewModel ViewModel => DataContext as ImageTagViewModel;

        private void _FocusOnVisible(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is IInputElement input)
            {
                Keyboard.Focus(input);
            }
        }

        private void _HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                ViewModel?.Refresh();
            }
        }

        private void _HandleTagTextKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel?.AddTagCommand.Execute();
            }
        }

        private void _HandleVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                ViewModel?.Refresh();
            }
        }
    }
}
