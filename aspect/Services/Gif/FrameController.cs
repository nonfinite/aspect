using System.Windows.Threading;

using Aspect.Utility;

using WpfAnimatedGif;

namespace Aspect.Services.Gif
{
    public sealed class FrameController : NotifyPropertyChanged, IFrameController
    {
        public bool IsPlaying { get; } = false;

        public void NextFrame() => this.Log().Warning("NoOp");

        public void Pause() => this.Log().Warning("NoOp");

        public void Play() => this.Log().Warning("NoOp");

        public void PrevFrame() => this.Log().Warning("NoOp");

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
