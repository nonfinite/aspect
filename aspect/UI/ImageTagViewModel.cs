using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Aspect.Models;
using Aspect.Services;
using Aspect.UI.Commands;
using Aspect.Utility;

namespace Aspect.UI
{
    public sealed class ImageTagViewModel : NotifyPropertyChanged
    {
        public ImageTagViewModel(FileData file, ITagService tagService)
        {
            mFile = file;
            mTagService = tagService;

            AddTagCommand = new DelegateCommand(_AddTag, () => !string.IsNullOrWhiteSpace(NewTag));
            DeleteTagCommand = new DelegateCommand<string>(_DeleteTag);
        }

        private readonly FileData mFile;
        private readonly ITagService mTagService;

        private string mNewTag;

        public DelegateCommand AddTagCommand { get; }

        public DelegateCommand<string> DeleteTagCommand { get; }

        public string NewTag
        {
            get => mNewTag;
            set
            {
                if (Set(ref mNewTag, value))
                {
                    AddTagCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<string> Tags { get; } = new ObservableCollection<string>();

        private async void _AddTag()
        {
            await AddTag(NewTag);
            NewTag = "";
        }

        private async void _DeleteTag(string tag)
        {
            if (Tags.Remove(tag))
            {
                await mTagService.RemoveTagFromFile(mFile, tag);
            }
        }

        public async Task AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }

            if (!Tags.Contains(tag))
            {
                Tags.Add(tag);
                await mTagService.AddTagToFile(mFile, tag);
            }
        }

        public async Task Refresh()
        {
            Tags.Clear();
            var tags = await mTagService.GetTagsForFile(mFile);
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }
    }
}
