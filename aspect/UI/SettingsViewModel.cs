using System.Collections.Generic;
using System.Threading.Tasks;

using Aspect.Properties;
using Aspect.Services;
using Aspect.Utility;

using Optional;

using Squirrel;

namespace Aspect.UI
{
    public sealed class SettingsViewModel : NotifyPropertyChanged
    {
        public SettingsViewModel()
        {
            mSettings = Settings.Default;
        }

        private readonly Settings mSettings;

        private readonly UpdateService mUpdateService = new UpdateService();

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

        public bool StableUpdatesOnly
        {
            get => !mSettings.UpdateToPreRelease;
            set
            {
                if (StableUpdatesOnly != value)
                {
                    OnPropertyChanging();
                    mSettings.UpdateToPreRelease = !value;
                    mSettings.Save();
                    OnPropertyChanged();
                }
            }
        }

        public bool UpdateAutomatically
        {
            get => mSettings.UpdateAutomatically;
            set
            {
                if (UpdateAutomatically != value)
                {
                    OnPropertyChanging();
                    mSettings.UpdateAutomatically = value;
                    mSettings.Save();
                    OnPropertyChanged();
                }
            }
        }

        public Task<Option<Dictionary<ReleaseEntry, string>>> CheckForUpdates() => mUpdateService.CheckForUpdates();
        public Task<Option<ReleaseEntry>> Update() => mUpdateService.Update(true);
    }
}
