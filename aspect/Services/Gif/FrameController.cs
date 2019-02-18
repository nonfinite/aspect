using System.Windows.Threading;

using Aspect.Utility;

using WpfAnimatedGif;

namespace Aspect.Services.Gif
{
    public class FrameController : NotifyPropertyChanged, IFrameController
    {
        private bool mIsPlaying;

        public virtual int CurrentFrame
        {
            get => 0;
            set { }
        }

        public bool IsPlaying
        {
            get => mIsPlaying;
            protected set => Set(ref mIsPlaying, value);
        }

        public virtual int MaxFrameNumber => 0;


        public virtual void NextFrame() => this.Log().Warning("NoOp");

        public virtual void Pause() => this.Log().Warning("NoOp");

        public virtual void Play() => this.Log().Warning("NoOp");

        public virtual void PrevFrame() => this.Log().Warning("NoOp");

        public virtual void Rewind() => this.Log().Warning("NoOp");

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
