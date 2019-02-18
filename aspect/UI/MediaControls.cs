using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Aspect.Utility;

namespace Aspect.UI
{
    public sealed class MediaElementControls : NotifyPropertyChanged
    {
        public MediaElementControls(Uri source)
        {
            mLoadedTask = new TaskCompletionSource<MediaElementControls>();

            mElement = new MediaElement
            {
                Stretch = Stretch.Uniform,
                LoadedBehavior = MediaState.Manual,
                ScrubbingEnabled = true,
            };
            mElement.MediaEnded += _HandleMediaEnded;
            mElement.MediaOpened += _HandleMediaOpened;
            mElement.Source = source;

            Brush = new VisualBrush(mElement);

            Play();
        }

        private readonly MediaElement mElement;
        private readonly TaskCompletionSource<MediaElementControls> mLoadedTask;
        private bool mIsPlaying;
        public Brush Brush { get; }
        public UIElement Element => mElement;

        public bool IsPlaying
        {
            get => mIsPlaying;
            set => Set(ref mIsPlaying, value);
        }

        public Task<MediaElementControls> LoadedTask => mLoadedTask.Task;

        private void _HandleMediaEnded(object sender, RoutedEventArgs e)
        {
            mElement.Position = new TimeSpan(0, 0, 1);
            Play();
        }

        private void _HandleMediaOpened(object sender, RoutedEventArgs e) => mLoadedTask.SetResult(this);

        public void Dispose() => mElement.MediaEnded -= _HandleMediaEnded;

        public void Pause()
        {
            mElement.Pause();
            IsPlaying = false;
        }

        public void Play()
        {
            mElement.Play();
            IsPlaying = true;
        }
    }
}
