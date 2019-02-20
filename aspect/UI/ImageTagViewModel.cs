using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

using Aspect.Models;
using Aspect.Services;
using Aspect.UI.Commands;

namespace Aspect.UI
{
    public sealed class ImageTagViewModel
    {
        public ImageTagViewModel(FileData file, ITagService tagService)
        {
            mFile = file;
            mTagService = tagService;

            DeleteTagCommand = new DelegateCommand<string>(_DeleteTag);
        }

        private readonly FileData mFile;
        private readonly ITagService mTagService;

        public ICommand DeleteTagCommand { get; }

        public ObservableCollection<string> Tags { get; } = new ObservableCollection<string>();

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
