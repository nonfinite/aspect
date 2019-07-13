using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

using Aspect.Models;
using Aspect.Utility;

using Serilog;

namespace Aspect.Services
{
    [DataContract(Name = "Settings", Namespace = "")]
    public sealed class Settings : NotifyPropertyChanged
    {
        private Settings()
        {
            mCanSave = false;
            _SetDefaults();
        }

        private static readonly Lazy<string> mFile = new Lazy<string>(() =>
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var aspect = Path.Combine(appData, "Aspect");
            Directory.CreateDirectory(aspect);
            return Path.Combine(aspect, "config.xml");
        });

        private bool mCanSave;
        private string mGitHubUpdateUrl;
        private bool mKeepImageOnScreen;
        private DateTimeOffset mLastUpdateCheck;
        private bool mMaximizeOnStartup;
        private bool mMultiImageSelection;
        private bool mShowMediaControls;
        private bool mShowThumbnails;
        private byte mSlideshowDurationInSeconds;
        private SortBy mSortBy;
        private TimeSpan mTimeBetweenUpdateChecks;
        private bool mUpdateAutomatically;
        private bool mUpdateToPreRelease;

        public static Settings Default { get; } = _Load();

        [DataMember(Name = "GitHubUpdateUrl", IsRequired = false, EmitDefaultValue = true)]
        public string GitHubUpdateUrl
        {
            get => mGitHubUpdateUrl;
            set => Set(ref mGitHubUpdateUrl, value);
        }

        [DataMember(Name = "KeepImageOnScreen", IsRequired = false, EmitDefaultValue = true)]
        public bool KeepImageOnScreen
        {
            get => mKeepImageOnScreen;
            set => Set(ref mKeepImageOnScreen, value);
        }

        [DataMember(Name = "LastUpdateCheck", IsRequired = false, EmitDefaultValue = true)]
        public DateTimeOffset LastUpdateCheck
        {
            get => mLastUpdateCheck;
            set => Set(ref mLastUpdateCheck, value);
        }

        [DataMember(Name = "MaximizeOnStartup", IsRequired = false, EmitDefaultValue = true)]
        public bool MaximizeOnStartup
        {
            get => mMaximizeOnStartup;
            set => Set(ref mMaximizeOnStartup, value);
        }

        [DataMember(Name = "MultiImageSelection", IsRequired = false, EmitDefaultValue = true)]
        public bool MultiImageSelection
        {
            get => mMultiImageSelection;
            set => Set(ref mMultiImageSelection, value);
        }

        [IgnoreDataMember] public DateTimeOffset NextUpdateCheck => LastUpdateCheck.Add(TimeBetweenUpdateChecks);

        [DataMember(Name = "ShowMediaControls", IsRequired = false, EmitDefaultValue = true)]
        public bool ShowMediaControls
        {
            get => mShowMediaControls;
            set => Set(ref mShowMediaControls, value);
        }

        [DataMember(Name = "ShowThumbnails", IsRequired = false, EmitDefaultValue = true)]
        public bool ShowThumbnails
        {
            get => mShowThumbnails;
            set => Set(ref mShowThumbnails, value);
        }

        [DataMember(Name = "SlideshowDurationInSeconds", IsRequired = false, EmitDefaultValue = true)]
        public byte SlideshowDurationInSeconds
        {
            get => mSlideshowDurationInSeconds;
            set => Set(ref mSlideshowDurationInSeconds, value);
        }

        [DataMember(Name = "SortBy", IsRequired = false, EmitDefaultValue = true)]
        public SortBy SortBy
        {
            get => mSortBy;
            set => Set(ref mSortBy, value);
        }

        [DataMember(Name = "TimeBetweenUpdateChecks", IsRequired = false, EmitDefaultValue = true)]
        public TimeSpan TimeBetweenUpdateChecks
        {
            get => mTimeBetweenUpdateChecks;
            set => Set(ref mTimeBetweenUpdateChecks, value);
        }

        [DataMember(Name = "UpdateAutomatically", IsRequired = false, EmitDefaultValue = true)]
        public bool UpdateAutomatically
        {
            get => mUpdateAutomatically;
            set => Set(ref mUpdateAutomatically, value);
        }

        [DataMember(Name = "UpdateToPreRelease", IsRequired = false, EmitDefaultValue = true)]
        public bool UpdateToPreRelease
        {
            get => mUpdateToPreRelease;
            set => Set(ref mUpdateToPreRelease, value);
        }

        private static DataContractSerializer _CreateSerializer() => new DataContractSerializer(typeof(Settings));

        private static Settings _Load()
        {
            Log.Information("Loading settings from {File}", mFile.Value);

            Settings settings;

            try
            {
                using (var stream = File.OpenRead(mFile.Value))
                {
                    var serializer = _CreateSerializer();
                    settings = (Settings) serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to deserialize settings from {File}", mFile.Value);
                settings = new Settings();
                settings._Save();
            }

            settings.mCanSave = true;
            return settings;
        }

        private void _Save()
        {
            this.Log().Information("Saving settings to {File}", mFile.Value);

            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8,
                NewLineOnAttributes = false
            };

            try
            {
                using (var writer = XmlWriter.Create(mFile.Value, xmlSettings))
                {
                    _CreateSerializer().WriteObject(writer, this);
                }
            }
            catch (Exception ex)
            {
                this.Log().Error(ex, "Failed to save settings to {File}", mFile.Value);
            }
        }

        private void _SetDefaults()
        {
            mGitHubUpdateUrl = "https://github.com/nonfinite/aspect";
            mKeepImageOnScreen = true;
            mLastUpdateCheck = DateTimeOffset.MinValue;
            mMaximizeOnStartup = true;
            mShowMediaControls = true;
            mShowThumbnails = false;
            mSlideshowDurationInSeconds = 15;
            mSortBy = SortBy.ModifiedDate;
            mTimeBetweenUpdateChecks = TimeSpan.FromDays(1);
            mUpdateAutomatically = true;
            mUpdateToPreRelease = false;
            mMultiImageSelection = false;
        }

        [OnDeserializing]
        internal void OnDeserializing(StreamingContext context) => _SetDefaults();

        protected override bool Set<TProperty>(ref TProperty field, TProperty value, [CallerMemberName] string propertyName = null)
        {
            if (base.Set(ref field, value, propertyName))
            {
                if (mCanSave)
                {
                    _Save();
                }

                return true;
            }

            return false;
        }
    }
}
