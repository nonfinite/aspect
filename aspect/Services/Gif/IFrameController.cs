using System.ComponentModel;

namespace Aspect.Services.Gif
{
    public interface IFrameController : INotifyPropertyChanged
    {
        bool IsPlaying { get; }
        void NextFrame();
        void Pause();
        void Play();
        void PrevFrame();
    }
}