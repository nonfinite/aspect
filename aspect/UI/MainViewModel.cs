using System;
using System.Threading.Tasks;
using System.Windows.Threading;

using Aspect.Models;
using Aspect.Properties;
using Aspect.Utility;

using Optional.Unsafe;

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
    }
}
