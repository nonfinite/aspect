using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

using Aspect.Services;
using Aspect.Utility;

using Optional;

namespace Aspect.Models
{
    public sealed class FileList : NotifyPropertyChanged
    {
        private FileList(FileData[] files, IPersistenceService persistence, ITagService tags)
        {
            mFiles = files;
            mPersistence = persistence;
            mTags = tags;

            View = CollectionViewSource.GetDefaultView(mFiles);
            View.Filter = _Filter;
            _ApplySort(Sort);

            Filter.PropertyChanged += _HandleFilterChanged;

            foreach (var file in files)
            {
                file.PropertyChanged += _HandleFileChanged;
            }
        }

        private readonly FileData[] mFiles;
        private readonly IPersistenceService mPersistence;
        private readonly ITagService mTags;

        public FileFilter Filter { get; } = new FileFilter();

        public bool IsPersistenceEnabled => mPersistence.IsEnabled;

        public SortBy Sort
        {
            get => Settings.Default.SortBy;
            set
            {
                if (Settings.Default.SortBy != value || View.SortDescriptions.Count == 0)
                {
                    Settings.Default.SortBy = value;

                    _ApplySort(value);
                }
            }
        }

        public ICollectionView View { get; }

        private void _ApplySort(SortBy value)
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

        private bool _Filter(object obj)
        {
            if (!(obj is FileData file))
            {
                return false;
            }

            return Filter.IsMatch(file);
        }

        private void _HandleFileChanged(object sender, PropertyChangedEventArgs e)
        {
            var file = (FileData) sender;

            switch (e.PropertyName)
            {
                case nameof(FileData.Rating):
                    mPersistence.UpdateRating(file);
                    break;
                default:
                    break;
            }
        }

        private void _HandleFilterChanged(object sender, PropertyChangedEventArgs e)
        {
            View.Refresh();
            if (View.CurrentItem == null)
            {
                View.MoveCurrentToFirst();
            }
        }

        private static async Task<Option<FileList>> _LoadDir(string directoryPath)
        {
            var files = await Task.Run(() =>
            {
                var fileList = new List<FileData>();
                foreach (var file in Directory.EnumerateFiles(directoryPath))
                {
                    FileData.From(file).MatchSome(fileList.Add);
                }

                return fileList.ToArray();
            });

            var persistence = await PersistenceService.Create(directoryPath).DontCaptureContext();
            await persistence.InitializeFiles(files).DontCaptureContext();
            var tags = await TagService.Create(persistence);

            return Option.Some(new FileList(files.ToArray(), persistence, tags));
        }

        private static async Task<Option<FileList>> _LoadFile(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            var option = await _LoadDir(dir);
            return option.Map(list =>
            {
                var data = list.mFiles.FirstOrDefault(file => file.IsFile(filePath));
                if (data != null)
                {
                    list.View.MoveCurrentTo(data);
                }

                return list;
            });
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

        public static Task<Option<FileList>> Load(string path)
        {
            if (File.Exists(path))
            {
                return _LoadFile(path);
            }

            if (Directory.Exists(path))
            {
                return _LoadDir(path);
            }

            return Task.FromResult(Option.None<FileList>());
        }
    }
}
