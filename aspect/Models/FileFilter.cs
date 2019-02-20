using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Aspect.Services;
using Aspect.Utility;

namespace Aspect.Models
{
    public sealed class FileFilter : NotifyPropertyChanged
    {
        public FileFilter(ITagService tagService, IPersistenceService persistenceService)
        {
            mTagService = tagService;
            mPersistenceService = persistenceService;
            mTextRegex = new LazyEx<Regex>(_CreateTextRegex);
            mTaggedFileIds = new LazyEx<Task<HashSet<long>>>(_GetTaggedFileIds);
        }

        private readonly IPersistenceService mPersistenceService;
        private readonly LazyEx<Task<HashSet<long>>> mTaggedFileIds;
        private readonly ITagService mTagService;
        private readonly LazyEx<Regex> mTextRegex;
        private Rating? mRating;
        private string mText;

        public Rating? Rating
        {
            get => mRating;
            set => Set(ref mRating, value);
        }

        public string Text
        {
            get => mText;
            set
            {
                if (Set(ref mText, value))
                {
                    mTextRegex.Reset();
                    mTaggedFileIds.Reset();
                }
            }
        }

        private Regex _CreateTextRegex()
        {
            var text = Text;
            if (string.IsNullOrWhiteSpace(text) || !text.Contains("*"))
            {
                return null;
            }

            var regex = Regex.Escape(text).Replace(@"\*", ".*");
            return new Regex($"^{regex}$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        private async Task<HashSet<long>> _GetTaggedFileIds()
        {
            var tag = Text?.Trim();
            if (!string.IsNullOrEmpty(tag))
            {
                var fileIds = await mTagService.GetFilesMatchingTag(tag);
                return new HashSet<long>(fileIds);
            }

            return new HashSet<long>();
        }

        public async Task<bool> IsMatch(FileData file)
        {
            if (file == null)
            {
                return false;
            }

            var rating = Rating;
            if (rating.HasValue)
            {
                var fr = file.Rating;
                if (!fr.HasValue)
                {
                    return false;
                }

                if (fr.Value < rating.Value)
                {
                    return false;
                }
            }

            var text = Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var taggedFileIds = await mTaggedFileIds.Value;
                if (taggedFileIds.Count > 0 &&
                    taggedFileIds.Contains(await mPersistenceService.GetFileId(file)))
                {
                    return true;
                }

                var regex = mTextRegex.Value;
                if (regex == null)
                {
                    if (file.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (!regex.IsMatch(file.Name))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
