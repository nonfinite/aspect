using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Aspect.Models;
using Aspect.Properties;
using Aspect.Services;
using Aspect.Utility;

using Microsoft.Win32;

using Optional;
using Optional.Unsafe;

using Squirrel;

namespace Aspect.UI
{
    public class MainViewModel : NotifyPropertyChanged
    {
        public MainViewModel()
        {
            _ResetSlideshowTimer();
            mSlideshowTimer = new DispatcherTimer(
                TimeSpan.FromSeconds(1), DispatcherPriority.Input,
                _HandleSlideshowTick, Dispatcher.CurrentDispatcher)
            {
                IsEnabled = false
            };
        }

        private readonly DispatcherTimer mSlideshowTimer;
        private readonly UpdateService mUpdateService = new UpdateService();
        private FileList mFileList;
        private byte mSlideshowSecondsRemaining;

        public Tuple<SortBy, string>[] AvailableSortBy { get; } =
        {
            Tuple.Create(SortBy.Name, "Name"),
            Tuple.Create(SortBy.ModifiedDate, "Modified"),
            Tuple.Create(SortBy.Size, "Size"),
            Tuple.Create(SortBy.Random, "Random"),
        };

        public FileList FileList
        {
            get => mFileList;
            private set => Set(ref mFileList, value);
        }

        public bool IsSlideshowRunning
        {
            get => mSlideshowTimer.IsEnabled;
            set
            {
                if (IsSlideshowRunning != value)
                {
                    OnPropertyChanging();
                    _ResetSlideshowTimer();
                    mSlideshowTimer.IsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte SlideshowSecondsRemaining
        {
            get => mSlideshowSecondsRemaining;
            private set => Set(ref mSlideshowSecondsRemaining, value);
        }

        private void _HandleSlideshowTick(object sender, EventArgs e)
        {
            if (SlideshowSecondsRemaining <= 1)
            {
                NavForward();
                _ResetSlideshowTimer();
            }
            else
            {
                SlideshowSecondsRemaining--;
            }
        }

        private void _HandleUpdateCompleted(Task<Option<ReleaseEntry>> task)
        {
            if (task.Exception != null)
            {
                this.Log().Error(task.Exception.Flatten(), "Automatic update failed");
                return;
            }

            task.Result.Match(
                release => this.Log().Information(
                    "Automatically updated to release {Version} - {SHA1} {BaseUrl} {FileName} {FileSize}",
                    release.Version, release.SHA1, release.BaseUrl, release.Filename, release.Filesize),
                () => this.Log().Information("No pending updates"));
        }

        private void _ResetSlideshowTimer() =>
            SlideshowSecondsRemaining = Math.Max(Settings.Default.SlideshowDurationInSeconds, (byte) 1);

        public async Task Initialize(string[] args)
        {
            foreach (var path in args)
            {
                this.Log().Information("Initializing from {Path}", path);
                var maybeFile = await FileList.Load(path);
                if (maybeFile.HasValue)
                {
                    FileList = maybeFile.ValueOrFailure();
                    break;
                }
            }

            if (FileList == null)
            {
                if (!await Open())
                {
                    Application.Current.Shutdown();
                }
            }

            await mUpdateService.Update(false).ContinueWith(_HandleUpdateCompleted).DontCaptureContext();
        }

        public void NavBack()
        {
            var fileList = FileList?.View;
            if (fileList != null)
            {
                if (!fileList.MoveCurrentToPrevious())
                {
                    // start of list
                    fileList.MoveCurrentToLast();
                }
            }
        }

        public void NavForward()
        {
            var fileList = FileList?.View;
            if (fileList != null)
            {
                if (!fileList.MoveCurrentToNext())
                {
                    // end of list
                    fileList.MoveCurrentToFirst();
                }
            }
        }

        public async Task<bool> Open()
        {
            var currentFile = (FileList?.View.CurrentItem as FileData)?.Uri.LocalPath;
            var currentDir = Path.GetDirectoryName(currentFile);

            var supportedExtensions = string.Join(";", FileData.SupportedFileExtensions
                .Select(ext => $"*{ext}")
                .OrderBy(ext => ext));
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DereferenceLinks = true,
                RestoreDirectory = true,
                Filter = $"Supported Images ({supportedExtensions})|{supportedExtensions}",
                FileName = Path.GetFileName(currentFile) ?? "",
                InitialDirectory = currentDir ?? "",
            };

            if (ofd.ShowDialog(Application.Current.MainWindow) ?? false)
            {
                var list = await FileList.Load(ofd.FileName);
                if (list.HasValue)
                {
                    FileList = list.ValueOr((FileList) null);

                    return FileList != null;
                }
            }

            return false;
        }
    }
}
