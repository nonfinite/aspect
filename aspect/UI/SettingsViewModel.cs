using System.IO;
using System.Threading.Tasks;

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
            UpdateService.Instance.GetCurrentVersion()
                .ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        this.Log().Error(task.Exception, "Failed to load version");
                        CurrentVersion = "unknown";
                    }
                    else
                    {
                        CurrentVersion = task.Result.ToString();
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private readonly Settings mSettings;

        private string mCurrentVersion = "...loading...";

        public string CurrentVersion
        {
            get => mCurrentVersion;
            private set => Set(ref mCurrentVersion, value);
        }

        public bool KeepImageOnScreen
        {
            get => mSettings.KeepImageOnScreen;
            set
            {
                if (KeepImageOnScreen != value)
                {
                    OnPropertyChanging();
                    mSettings.KeepImageOnScreen = value;
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
                    OnPropertyChanged();
                }
            }
        }

        public string ReleaseNotes
        {
            get
            {
                var releaseNotes = _GetResourceContents("Aspect.Properties.release-notes.md");
                var credits = _GetResourceContents("Aspect.Properties.credits.md");

                return $"{releaseNotes}\r\n\r\n{credits}";
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
                    OnPropertyChanged();
                }
            }
        }

        private string _GetResourceContents(string name)
        {
            using (var stream = typeof(App).Assembly.GetManifestResourceStream(name))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public Task<Option<ReleaseEntry>> CheckForUpdates() => UpdateService.Instance.CheckForUpdates();

        public Task<Option<ReleaseEntry>> Update() => UpdateService.Instance.Update(true);
    }
}
