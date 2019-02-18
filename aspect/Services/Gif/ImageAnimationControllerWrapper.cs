using System;
using System.Windows.Threading;

using WpfAnimatedGif;

namespace Aspect.Services.Gif
{
    public sealed class ImageAnimationControllerWrapper : FrameController, IFrameController
    {
        public ImageAnimationControllerWrapper(ImageAnimationController controller, Dispatcher dispatcher)
        {
            mController = controller;
            mDispatcher = dispatcher;
            mController.CurrentFrameChanged += _HandleCurrentFrameChanged;
        }

        private readonly ImageAnimationController mController;
        private readonly Dispatcher mDispatcher;

        public override int CurrentFrame => mController.CurrentFrame;
        public override int FrameCount => mController.FrameCount;

        public override void NextFrame()
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

        public override void Pause()
        {
            if (IsPlaying && mController != null)
            {
                mController.Pause();
                IsPlaying = false;
            }
        }

        public override void Play()
        {
            if (!IsPlaying && mController != null)
            {
                mController.Play();
                IsPlaying = true;
            }
        }

        public override void PrevFrame()
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

        private void _HandleCurrentFrameChanged(object sender, EventArgs e) => OnPropertyChanged(nameof(CurrentFrame));
    }
}
