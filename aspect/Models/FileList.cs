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
        }

        private readonly FileData[] mFiles;
        public ICollectionView View { get; }

        public static Option<FileList> Load(string path)
        {
            return LoadFile(path).Else(() => LoadDir(path));
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
    }
}
