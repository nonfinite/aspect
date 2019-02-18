using System.Windows.Threading;

using Aspect.Utility;

using WpfAnimatedGif;

namespace Aspect.Services.Gif
{
    public class FrameController : NotifyPropertyChanged, IFrameController
    {
        private bool mIsPlaying;
        public virtual int CurrentFrame => 0;
        public virtual int FrameCount => 0;

        public bool IsPlaying
        {
            get => mIsPlaying;
            protected set => Set(ref mIsPlaying, value);
        }


        public virtual void NextFrame() => this.Log().Warning("NoOp");

        public virtual void Pause() => this.Log().Warning("NoOp");

        public virtual void Play() => this.Log().Warning("NoOp");

        public virtual void PrevFrame() => this.Log().Warning("NoOp");

        public static IFrameController Create(ImageAnimationController controller, Dispatcher dispatcher)
        {
            if (controller != null)
            {
                return new ImageAnimationControllerWrapper(controller, dispatcher);
            }

            return new FrameController();
        }
    }
}
