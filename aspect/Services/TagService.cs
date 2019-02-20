using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Aspect.Models;
using Aspect.Services.DbModels;

namespace Aspect.Services
{
    public interface ITagService
    {
        bool IsEnabled { get; }
        Task AddTagToFile(FileData file, string tag);
        Task<long[]> GetFilesMatchingTag(string tag);
        Task<string[]> GetTagsForFile(FileData file);
        Task RemoveTagFromFile(FileData file, string tag);
    }

    public sealed class TagService : ITagService
    {
        private TagService(IPersistenceService persistence, IEnumerable<TagRow> tags)
        {
            mPersistence = persistence;
            mTagIds = new SortedList<string, long>(StringComparer.OrdinalIgnoreCase);
            foreach (var tag in tags)
            {
                mTagIds.Add(tag.Name, tag.Id);
            }
        }

        private readonly IPersistenceService mPersistence;
        private readonly SortedList<string, long> mTagIds;

        public async Task AddTagToFile(FileData file, string tag)
        {
            var tagId = await _GetTagId(tag);
            await mPersistence.AddTagToFile(file, tagId);
        }

        public Task<long[]> GetFilesMatchingTag(string tag)
        {
            if (!mTagIds.TryGetValue(tag, out var tagId))
            {
                return Task.FromResult(new long[0]);
            }

            return mPersistence.GetFilesWithTag(tagId);
        }

        public Task<string[]> GetTagsForFile(FileData file) => mPersistence.GetTagsForFile(file);

        public bool IsEnabled => mPersistence.IsEnabled;

        public async Task RemoveTagFromFile(FileData file, string tag)
        {
            var tagId = await _GetTagId(tag);
            await mPersistence.RemoveTagFromFile(file, tagId);
        }

        private async Task<long> _GetTagId(string tag)
        {
            if (!mTagIds.TryGetValue(tag, out var tagId))
            {
                tagId = await mPersistence.CreateTag(tag);
                mTagIds[tag] = tagId;
            }

            return tagId;
        }

        public static async Task<ITagService> Create(IPersistenceService persistence)
        {
            var tags = await persistence.GetAllTags();
            return new TagService(persistence, tags);
        }
    }
}
