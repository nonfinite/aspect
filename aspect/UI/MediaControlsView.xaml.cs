using System.Windows;
using System.Windows.Controls;

namespace Aspect.UI
{
    public partial class MediaControlsView : UserControl
    {
        public MediaControlsView()
        {
            InitializeComponent();
        }

        public MediaElementControls Controls => DataContext as MediaElementControls;

        private void _TogglePlayPause(object sender, RoutedEventArgs e)
        {
            var controls = Controls;
            if (controls == null)
            {
                return;
            }

            if (controls.IsPlaying)
            {
                controls.Pause();
            }
            else
            {
                controls.Play();
            }
        }
    }
}
