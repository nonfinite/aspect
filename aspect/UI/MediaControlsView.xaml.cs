using System.Windows;
using System.Windows.Controls;

using Aspect.Services.Gif;

namespace Aspect.UI
{
    public partial class MediaControlsView : UserControl
    {
        public MediaControlsView()
        {
            InitializeComponent();
        }

        public IFrameController Controls => DataContext as IFrameController;

        private void _NextFrame(object sender, RoutedEventArgs e) => Controls.NextFrame();

        private void _PrevFrame(object sender, RoutedEventArgs e) => Controls.PrevFrame();

        private void _TogglePlayPause(object sender, RoutedEventArgs e)
        {
            var controls = Controls;

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
