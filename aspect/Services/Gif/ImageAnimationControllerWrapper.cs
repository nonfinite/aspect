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

        public override int CurrentFrame
        {
            get => mController.CurrentFrame;
            set
            {
                if (value != CurrentFrame)
                {
                    OnPropertyChanging();
                    mController.GotoFrame(value % mController.FrameCount);
                    OnPropertyChanged();
                }
            }
        }

        public override int MaxFrameNumber => mController.FrameCount - 1;

        public override void NextFrame()
        {
            Pause();
            mDispatcher.Invoke(() =>
            {
                var index = (mController.CurrentFrame + 1) % mController.FrameCount;
                mController.GotoFrame(index);
            }, DispatcherPriority.Input);
        }

        public override void Pause()
        {
            if (IsPlaying)
            {
                mController.Pause();
                IsPlaying = false;
            }
        }

        public override void Play()
        {
            if (!IsPlaying)
            {
                mController.Play();
                IsPlaying = true;
            }
        }

        public override void PrevFrame()
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

        public override void Rewind()
        {
            Pause();
            mDispatcher.Invoke(() => { mController.GotoFrame(0); }, DispatcherPriority.Input);
        }

        private void _HandleCurrentFrameChanged(object sender, EventArgs e) => OnPropertyChanged(nameof(CurrentFrame));
    }
}
