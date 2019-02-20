using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Aspect.Models;
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

        private ImageTagViewModel mCurrentItemTagViewModel;
        private FileList mFileList;
        private byte mSlideshowSecondsRemaining;
        private Task mUpdateTask;

        public Tuple<SortBy, string>[] AvailableSortBy { get; } =
        {
            Tuple.Create(SortBy.ModifiedDate, "Modified Date"),
            Tuple.Create(SortBy.Name, "Name"),
            Tuple.Create(SortBy.Random, "Random"),
            Tuple.Create(SortBy.Size, "Size"),
        };

        public ImageTagViewModel CurrentItemTagViewModel
        {
            get => mCurrentItemTagViewModel;
            private set => Set(ref mCurrentItemTagViewModel, value);
        }

        public FileList FileList
        {
            get => mFileList;
            private set
            {
                if (mFileList != value)
                {
                    if (mFileList != null)
                    {
                        mFileList.View.CurrentChanged -= _HandleCurrentFileChanged;
                    }

                    mFileList = value;
                    OnPropertyChanged();

                    if (value != null)
                    {
                        value.View.CurrentChanged += _HandleCurrentFileChanged;
                    }

                    _HandleCurrentFileChanged(FileList, EventArgs.Empty);
                }
            }
        }


        public bool IsSlideshowRunning
        {
            get => mSlideshowTimer.IsEnabled;
            set
            {
                if (IsSlideshowRunning != value)
                {
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

        private void _HandleCurrentFileChanged(object sender, EventArgs e)
        {
            if (FileList?.View.CurrentItem is FileData file)
            {
                CurrentItemTagViewModel = new ImageTagViewModel(file, FileList.TagService);
            }
            else
            {
                CurrentItemTagViewModel = null;
            }
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

            mUpdateTask = null;
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

            mUpdateTask = UpdateService.Instance.Update(false)
                .ContinueWith(_HandleUpdateCompleted);
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
