using System;
using System.IO;
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
        }

        private static readonly Lazy<string> mFile = new Lazy<string>(() =>
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var aspect = Path.Combine(appData, "Aspect");
            Directory.CreateDirectory(aspect);
            return Path.Combine(aspect, "config.xml");
        });

        private bool mCanSave;

        private string mGitHubUpdateUrl = "https://github.com/nonfinite/aspect";
        private bool mKeepImageOnScreen = true;
        private bool mMaximizeOnStartup = true;
        private bool mShowThumbnails;
        private byte mSlideshowDurationInSeconds = 15;
        private SortBy mSortBy = SortBy.ModifiedDate;
        private bool mUpdateAutomatically = true;
        private bool mUpdateToPreRelease = false;

        public static Settings Default { get; } = _Load();

        [DataMember(Name = "GitHubUpdateUrl", IsRequired = true, EmitDefaultValue = true)]
        public string GitHubUpdateUrl
        {
            get => mGitHubUpdateUrl;
            set => Set(ref mGitHubUpdateUrl, value);
        }

        [DataMember(Name = "KeepImageOnScreen", IsRequired = true, EmitDefaultValue = true)]
        public bool KeepImageOnScreen
        {
            get => mKeepImageOnScreen;
            set => Set(ref mKeepImageOnScreen, value);
        }

        [DataMember(Name = "MaximizeOnStartup", IsRequired = true, EmitDefaultValue = true)]
        public bool MaximizeOnStartup
        {
            get => mMaximizeOnStartup;
            set => Set(ref mMaximizeOnStartup, value);
        }

        [DataMember(Name = "ShowThumbnails", IsRequired = true, EmitDefaultValue = true)]
        public bool ShowThumbnails
        {
            get => mShowThumbnails;
            set => Set(ref mShowThumbnails, value);
        }

        [DataMember(Name = "SlideshowDurationInSeconds", IsRequired = true, EmitDefaultValue = true)]
        public byte SlideshowDurationInSeconds
        {
            get => mSlideshowDurationInSeconds;
            set => Set(ref mSlideshowDurationInSeconds, value);
        }

        [DataMember(Name = "SortBy", IsRequired = true, EmitDefaultValue = true)]
        public SortBy SortBy
        {
            get => mSortBy;
            set => Set(ref mSortBy, value);
        }

        [DataMember(Name = "UpdateAutomatically", IsRequired = true, EmitDefaultValue = true)]
        public bool UpdateAutomatically
        {
            get => mUpdateAutomatically;
            set => Set(ref mUpdateAutomatically, value);
        }

        [DataMember(Name = "UpdateToPreRelease", IsRequired = true, EmitDefaultValue = true)]
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

        protected override bool Set<TProperty>(ref TProperty field, TProperty value, string propertyName = null)
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
