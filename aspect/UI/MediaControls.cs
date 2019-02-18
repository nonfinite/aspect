using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using Aspect.Models;
using Aspect.Utility;

using WpfAnimatedGif;

namespace Aspect.UI
{
    public sealed class MediaElementControls : NotifyPropertyChanged
    {
        public MediaElementControls(FileData file)
        {
            mLoadedTask = new TaskCompletionSource<MediaElementControls>();

            mDimensions = file.Dimensions;
            mImage = new Image
            {
                Stretch = Stretch.Uniform,
                Width = file.Dimensions.Width,
                Height = file.Dimensions.Height,
            };
            mImage.DpiChanged += _HandleDpiChanged;
            ImageBehavior.AddAnimationLoadedHandler(mImage, _HandleAnimationLoaded);
            ImageBehavior.SetAutoStart(mImage, false);
            ImageBehavior.SetRepeatBehavior(mImage, RepeatBehavior.Forever);
            ImageBehavior.SetAnimatedSource(mImage, new BitmapImage(file.Uri));

            Brush = new VisualBrush(mImage);
        }

        private readonly Size mDimensions;
        private readonly Image mImage;
        private readonly TaskCompletionSource<MediaElementControls> mLoadedTask;
        private ImageAnimationController mController;
        private bool mIsPlaying;
        public Brush Brush { get; }
        public UIElement Element => mImage;

        public bool IsPlaying
        {
            get => mIsPlaying;
            set => Set(ref mIsPlaying, value);
        }

        public Task<MediaElementControls> LoadedTask => mLoadedTask.Task;

        private void _HandleAnimationLoaded(object sender, RoutedEventArgs e)
        {
            mController = ImageBehavior.GetAnimationController(mImage);
            mLoadedTask.SetResult(this);
            Play();
        }

        private void _HandleDpiChanged(object sender, DpiChangedEventArgs e)
        {
            mImage.Width = mDimensions.Width / e.NewDpi.DpiScaleX;
            mImage.Height = mDimensions.Height / e.NewDpi.DpiScaleY;
        }

        public void Dispose() => ImageBehavior.RemoveAnimationLoadedHandler(mImage, _HandleAnimationLoaded);

        public void Pause()
        {
            if (IsPlaying && mController != null)
            {
                mController.Pause();
                IsPlaying = false;
            }
        }

        public void Play()
        {
            if (!IsPlaying && mController != null)
            {
                mController.Play();
                IsPlaying = true;
            }
        }
    }
}
