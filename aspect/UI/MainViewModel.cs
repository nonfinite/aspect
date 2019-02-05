using System;
using System.Threading.Tasks;

using Aspect.Models;
using Aspect.Utility;

using Optional.Unsafe;

namespace Aspect.UI
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private FileList mFileList;

        public Tuple<SortBy, string>[] AvailableSortBy { get; } = new[]
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
