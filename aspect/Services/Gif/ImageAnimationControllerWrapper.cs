using System.Windows.Threading;

using Aspect.Utility;

using WpfAnimatedGif;

namespace Aspect.Services.Gif
{
    public sealed class ImageAnimationControllerWrapper : NotifyPropertyChanged, IFrameController
    {
        public ImageAnimationControllerWrapper(ImageAnimationController controller, Dispatcher dispatcher)
        {
            mController = controller;
            mDispatcher = dispatcher;
        }

        private readonly ImageAnimationController mController;
        private readonly Dispatcher mDispatcher;
        private bool mIsPlaying;

        public bool IsPlaying
        {
            get => mIsPlaying;
            private set => Set(ref mIsPlaying, value);
        }

        public void NextFrame()
        {
            if (mController != null)
            {
                Pause();
                mDispatcher.Invoke(() =>
                {
                    var index = (mController.CurrentFrame + 1) % mController.FrameCount;
                    mController.GotoFrame(index);
                }, DispatcherPriority.Input);
            }
        }

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

        public void PrevFrame()
        {
            if (mController != null)
            {
                Pause();
                mDispatcher.Invoke(() =>
                {
                    var index = mController.CurrentFrame == 0
                        ? mController.FrameCount
                        : mController.CurrentFrame;

                    mController.GotoFrame(index - 1);
                }, DispatcherPriority.Input);
            }
        }
    }
}
