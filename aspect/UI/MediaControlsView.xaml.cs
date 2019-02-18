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

        private void _NextFrame(object sender, RoutedEventArgs e) => Controls.Controls.NextFrame();

        private void _PrevFrame(object sender, RoutedEventArgs e) => Controls.Controls.PrevFrame();

        private void _TogglePlayPause(object sender, RoutedEventArgs e)
        {
            var controls = Controls.Controls;

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
