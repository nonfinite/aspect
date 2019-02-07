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
                    OnPropertyChanging();
                    mSettings.KeepImageOnScreen = value;
                    mSettings.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool MaximizeOnStartup
        {
            get => mSettings.MaximizeOnStartup;
            set
            {
                if (MaximizeOnStartup != value)
                {
                    OnPropertyChanging();
                    mSettings.MaximizeOnStartup = value;
                    mSettings.Save();
                    OnPropertyChanged();
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
                    OnPropertyChanging();
                    mSettings.SlideshowDurationInSeconds = value;
                    mSettings.Save();
                    OnPropertyChanged();
                }
            }
        }
    }
}