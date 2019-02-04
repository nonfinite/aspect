using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
            View.Filter = _Filter;
            View.SortDescriptions.Add(new SortDescription(nameof(FileData.Name), ListSortDirection.Ascending));

            Filter.PropertyChanged += _HandleFilterChanged;
        }

        private readonly FileData[] mFiles;
        private SortBy mSort;

        public FileFilter Filter { get; } = new FileFilter();

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


        private bool _Filter(object obj)
        {
            if (!(obj is FileData file))
            {
                return false;
            }

            return Filter.IsMatch(file);
        }

        private void _HandleFilterChanged(object sender, PropertyChangedEventArgs e)
        {
            View.Refresh();
            if (View.CurrentItem == null)
            {
                View.MoveCurrentToFirst();
            }
        }

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
