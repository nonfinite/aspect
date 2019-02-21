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

            AvailableTags = new ObservableCollection<string>();
        }

        private readonly FileData mFile;
        private readonly ITagService mTagService;

        private string mNewTag;

        public DelegateCommand AddTagCommand { get; }

        public ObservableCollection<string> AvailableTags { get; }

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
                AvailableTags.Add(tag);
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
                AvailableTags.Remove(tag);
                await mTagService.AddTagToFile(mFile, tag);
            }
        }

        public async Task Refresh()
        {
            AvailableTags.Clear();
            foreach (var tag in mTagService.KnownTags)
            {
                AvailableTags.Add(tag);
            }

            Tags.Clear();
            var tags = await mTagService.GetTagsForFile(mFile);
            foreach (var tag in tags)
            {
                Tags.Add(tag);
                AvailableTags.Remove(tag);
            }
        }
    }
}
