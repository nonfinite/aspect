using System.ComponentModel;

namespace Aspect.Services.Gif
{
    public interface IFrameController : INotifyPropertyChanged
    {
        int CurrentFrame { get; }
        int FrameCount { get; }
        bool IsPlaying { get; }
        void NextFrame();
        void Pause();
        void Play();
        void PrevFrame();
    }
}
