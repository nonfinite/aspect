using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

using Aspect.Utility;

using Optional;

namespace Aspect.Models
{
    public sealed class FileList : NotifyPropertyChanged
    {
        private FileList(FileData[] files)
        {
            mFiles = files;
            View = CollectionViewSource.GetDefaultView(mFiles);
            View.SortDescriptions.Add(new SortDescription(nameof(FileData.Name), ListSortDirection.Ascending));
        }

        private readonly FileData[] mFiles;

        private Rating? mFilterRating;
        private SortBy mSort;

        public Rating? FilterRating
        {
            get => mFilterRating;
            set => _SetFilter(ref mFilterRating, value);
        }

        public SortBy Sort
        {
            get => mSort;
            set
            {
                if (Set(ref mSort, value) || View.SortDescriptions.Count == 0)
                {
                    using (View.DeferRefresh())
                    {
                        View.SortDescriptions.Clear();
                        string sortProperty;
                        switch (value)
                        {
                            case SortBy.Name:
                                sortProperty = nameof(FileData.Name);
                                break;
                            case SortBy.ModifiedDate:
                                sortProperty = nameof(FileData.ModifiedInstant);
                                break;
                            case SortBy.Size:
                                sortProperty = nameof(FileData.Size);
                                break;
                            case SortBy.Random:
                                sortProperty = nameof(FileData.RandomKey);
                                _ResetRandomKeys();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(value), value, null);
                        }

                        View.SortDescriptions.Add(new SortDescription(sortProperty, ListSortDirection.Ascending));
                    }
                }
            }
        }

        public ICollectionView View { get; }

        private void _ResetRandomKeys()
        {
            var rnd = new Random();
            var randomKeys = Enumerable.Range(0, mFiles.Length).ToList();
            foreach (var file in mFiles)
            {
                var idx = rnd.Next(0, randomKeys.Count);
                file.RandomKey = randomKeys[idx];
                randomKeys.RemoveAt(idx);
            }
        }

        private void _SetFilter<TProperty>(ref TProperty field, TProperty value,
            [CallerMemberName] string propertyName = null)
        {
            if (Set(ref field, value, propertyName))
            {
                View.Refresh();
            }
        }

        public static Option<FileList> Load(string path) => LoadFile(path).Else(() => LoadDir(path));

        public static Option<FileList> LoadDir(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return Option.None<FileList>();
            }

            var files = new List<FileData>();
            foreach (var file in Directory.EnumerateFiles(directoryPath))
            {
                FileData.From(file).MatchSome(files.Add);
            }

            return Option.Some(new FileList(files.ToArray()));
        }

        public static Option<FileList> LoadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Option.None<FileList>();
            }

            var dir = Path.GetDirectoryName(filePath);
            return LoadDir(dir).Map(list =>
            {
                var data = list.mFiles.FirstOrDefault(file => file.IsFile(filePath));
                if (data != null)
                {
                    list.View.MoveCurrentTo(data);
                }

                return list;
            });
        }
    }
}
