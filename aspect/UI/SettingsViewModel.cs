using Aspect.Properties;
using Aspect.Utility;

namespace Aspect.UI
{
    public sealed class SettingsViewModel : NotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            mSettings = Settings.Default;
        }

        private readonly Settings mSettings;

        public bool KeepImageOnScreen
        {
            get => mSettings.KeepImageOnScreen;
            set
            {
                if (KeepImageOnScreen != value)
                {
                    mSettings.KeepImageOnScreen = value;
                    mSettings.Save();
                }
            }
        }

        public byte SlideshowDurationInSeconds
        {
            get => mSettings.SlideshowDurationInSeconds;
            set
            {
                if (SlideshowDurationInSeconds != value)
                {
                    mSettings.SlideshowDurationInSeconds = value;
                    mSettings.Save();
                }
            }
        }
    }
}
