using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Aspect.Models;
using Aspect.Services.Gif;
using Aspect.Utility;

using WpfAnimatedGif;

namespace Aspect.UI
{
    public sealed class MediaElementControls : NotifyPropertyChanged
    {
        public MediaElementControls(FileData file)
        {
            mLoadedTask = new TaskCompletionSource<MediaElementControls>();
            mFile = file;

            App.FirstChanceDispatcherException += _HandleDispatcherException;

            mDimensions = file.Metadata.Dimensions;
            mImage = new Image
            {
                Stretch = Stretch.Uniform,
                Width = file.Metadata.Dimensions.Width,
                Height = file.Metadata.Dimensions.Height,
            };
            mImage.DpiChanged += _HandleDpiChanged;
            ImageBehavior.AddAnimationLoadedHandler(mImage, _HandleAnimationLoaded);
            ImageBehavior.SetAutoStart(mImage, false);
            ImageBehavior.SetRepeatBehavior(mImage, RepeatBehavior.Forever);
            ImageBehavior.SetAnimatedSource(mImage, new BitmapImage(file.Uri));

            Brush = new VisualBrush(mImage);
        }

        private readonly Size mDimensions;
        private readonly FileData mFile;
        private readonly Image mImage;
        private readonly TaskCompletionSource<MediaElementControls> mLoadedTask;
        private IFrameController mControls;
        public Brush Brush { get; }

        public IFrameController Controls
        {
            get => mControls;
            private set => Set(ref mControls, value);
        }

        public UIElement Element => mImage;
        public Task<MediaElementControls> LoadedTask => mLoadedTask.Task;

        private void _HandleAnimationLoaded(object sender, RoutedEventArgs e)
        {
            App.FirstChanceDispatcherException -= _HandleDispatcherException;
            Controls = FrameController.Create(ImageBehavior.GetAnimationController(mImage), mImage.Dispatcher);
            mLoadedTask.SetResult(this);
            Controls.Play();
        }

        private void _HandleDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            App.FirstChanceDispatcherException -= _HandleDispatcherException;

            var source = ImageBehavior.GetAnimatedSource(mImage);
            ImageBehavior.SetAnimatedSource(mImage, null);
            GC.Collect();
            mImage.Source = source;

            this.Log().Error(e.Exception,
                "Failed to load animated image from {File}, falling back to basic image.",
                mFile.Uri);

            e.Handled = true;
            mLoadedTask.SetResult(this);
        }

        private void _HandleDpiChanged(object sender, DpiChangedEventArgs e)
        {
            mImage.Width = mDimensions.Width / e.NewDpi.DpiScaleX;
            mImage.Height = mDimensions.Height / e.NewDpi.DpiScaleY;
        }

        public void Dispose() => ImageBehavior.RemoveAnimationLoadedHandler(mImage, _HandleAnimationLoaded);
    }
}
