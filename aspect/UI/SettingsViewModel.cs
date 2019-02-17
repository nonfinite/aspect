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
            Settings = Settings.Default;
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
            UpdateService.Instance.IsUpdatingSupported.ContinueWith(
                task => IsUpdatingSupported = task.Exception == null && task.Result,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private string mCurrentVersion = "...loading...";
        private bool mIsUpdatingSupported;


        public string CurrentVersion
        {
            get => mCurrentVersion;
            private set => Set(ref mCurrentVersion, value);
        }

        public bool IsUpdatingSupported
        {
            get => mIsUpdatingSupported;
            private set => Set(ref mIsUpdatingSupported, value);
        }

        public bool KeepImageOnScreen
        {
            get => Settings.KeepImageOnScreen;
            set
            {
                if (KeepImageOnScreen != value)
                {
                    OnPropertyChanging();
                    Settings.KeepImageOnScreen = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool MaximizeOnStartup
        {
            get => Settings.MaximizeOnStartup;
            set
            {
                if (MaximizeOnStartup != value)
                {
                    OnPropertyChanging();
                    Settings.MaximizeOnStartup = value;
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

        public Settings Settings { get; }

        public byte SlideshowDurationInSeconds
        {
            get => Settings.SlideshowDurationInSeconds;
            set
            {
                if (SlideshowDurationInSeconds != value)
                {
                    OnPropertyChanging();
                    Settings.SlideshowDurationInSeconds = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool StableUpdatesOnly
        {
            get => !Settings.UpdateToPreRelease;
            set
            {
                if (StableUpdatesOnly != value)
                {
                    OnPropertyChanging();
                    Settings.UpdateToPreRelease = !value;
                    OnPropertyChanged();
                }
            }
        }

        public bool UpdateAutomatically
        {
            get => Settings.UpdateAutomatically;
            set
            {
                if (UpdateAutomatically != value)
                {
                    OnPropertyChanging();
                    Settings.UpdateAutomatically = value;
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
