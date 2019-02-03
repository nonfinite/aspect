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

        public FileList FileList
        {
            get => mFileList;
            private set => Set(ref mFileList, value);
        }

        public Tuple<SortBy, string>[] AvailableSortBy { get; } = new[]
        {
            Tuple.Create(SortBy.Name, "Name"),
            Tuple.Create(SortBy.ModifiedDate, "Modified"),
            Tuple.Create(SortBy.Size, "Size"),
            Tuple.Create(SortBy.Random, "Random"),
        };

        public async Task Initialize(string[] args)
        {
            await Task.Run(() =>
            {
                foreach (var path in args)
                {
                    var maybeFile = FileList.Load(path);
                    if (maybeFile.HasValue)
                    {
                        FileList = maybeFile.ValueOrFailure();
                        break;
                    }
                }
            });
        }
    }
}
