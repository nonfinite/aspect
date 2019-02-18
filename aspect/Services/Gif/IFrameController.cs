using System.ComponentModel;

namespace Aspect.Services.Gif
{
    public interface IFrameController : INotifyPropertyChanged
    {
        int CurrentFrame { get; set; }
        bool IsPlaying { get; }
        int MaxFrameNumber { get; }
        void NextFrame();
        void Pause();
        void Play();
        void PrevFrame();
        void Rewind();
    }
}
