using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Aspect.Models;
using Aspect.Services.DbModels;
using Aspect.Utility;

using Dapper;

using DbUp;
using DbUp.Engine;
using DbUp.SQLite.Helpers;

namespace Aspect.Services
{
    public interface IPersistenceService
    {
        bool IsEnabled { get; }

        Task AddTagToFile(FileData file, long tagId);

        Task<long> CreateTag(string name);

        Task<TagRow[]> GetAllTags();

        Task<long> GetFileId(FileData file);
        Task<long[]> GetFilesWithTag(long tagId);

        Task<string[]> GetTagsForFile(FileData file);

        Task InitializeFiles(IEnumerable<FileData> files);

        Task RemoveTagFromFile(FileData file, long tagId);

        Task UpdateRating(FileData file);
    }

    public sealed class PersistenceService : IPersistenceService
    {
        private PersistenceService(string connectionString)
        {
            mConnectionString = connectionString;
        }

        private readonly string mConnectionString;

        public async Task AddTagToFile(FileData file, long tagId)
        {
            using (var connection = _Connect())
            {
                var fileId = await GetFileId(file);
                await connection.ExecuteAsync(
                    @"INSERT OR IGNORE INTO FileTag (file_id, tag_id) VALUES (@FileId, @TagId)",
                    new FileTagRow {FileId = fileId, TagId = tagId});
            }
        }

        public async Task<long> CreateTag(string name)
        {
            using (var connection = _Connect())
            {
                return await connection.ExecuteScalarAsync<long>(@"
INSERT OR IGNORE INTO Tag (name) VALUES (@name);
SELECT id FROM Tag WHERE name = @name", new TagRow {Name = name});
            }
        }

        public async Task<TagRow[]> GetAllTags()
        {
            using (var connection = _Connect())
            {
                var tags = await connection.QueryAsync<TagRow>("SELECT * FROM Tag");
                return tags.ToArray();
            }
        }

        public async Task<long> GetFileId(FileData file)
        {
            if (file.Id.HasValue)
            {
                return file.Id.Value;
            }

            using (var connection = _Connect())
            {
                var id = await connection.ExecuteScalarAsync<long>(@"
INSERT OR IGNORE INTO File (name, rating) VALUES (@name, @rating);
SELECT id FROM File WHERE name = @name", new FileRow
                {
                    Name = file.Name,
                    Rating = file.Rating?.Value
                });
                file.Id = id;
                return id;
            }
        }

        public async Task<long[]> GetFilesWithTag(long tagId)
        {
            using (var connection = _Connect())
            {
                var ids = await connection.QueryAsync<long>(
                    @"SELECT file_id FROM FileTag WHERE tag_id = @TagId",
                    new {TagId = tagId});
                return ids.ToArray();
            }
        }

        public async Task<string[]> GetTagsForFile(FileData file)
        {
            using (var connection = _Connect())
            {
                var id = await GetFileId(file);
                var tags = await connection.QueryAsync<string>(@"
SELECT name
  FROM FileTag
  JOIN Tag ON id = tag_id
 WHERE file_id = @FileId", new {FileId = id});
                return tags.ToArray();
            }
        }

        public async Task InitializeFiles(IEnumerable<FileData> files)
        {
            var filesByName = files.ToDictionary(file => file.Name, StringComparer.OrdinalIgnoreCase);

            using (var connection = _Connect())
            {
                var rows = await connection.QueryAsync<FileRow>("SELECT * FROM File");
                foreach (var row in rows)
                {
                    if (filesByName.TryGetValue(row.Name, out var file))
                    {
                        file.Id = row.Id;
                        file.Rating = row.Rating.HasValue ? new Rating(row.Rating.Value) : (Rating?) null;
                    }
                }
            }
        }


        public bool IsEnabled => true;

        public async Task RemoveTagFromFile(FileData file, long tagId)
        {
            using (var connection = _Connect())
            {
                var fileId = await GetFileId(file);
                await connection.ExecuteAsync(
                    @"DELETE FROM FileTag WHERE file_id = @FileId AND tag_id = @TagId",
                    new FileTagRow {FileId = fileId, TagId = tagId});
            }
        }

        public async Task UpdateRating(FileData file)
        {
            using (var connection = _Connect())
            {
                await connection.ExecuteAsync(@"
INSERT INTO File   ( name,  rating)
            VALUES (@name, @rating)
ON CONFLICT (name)
DO UPDATE SET rating = @rating", new
                {
                    name = file.Name,
                    rating = file.Rating?.Value
                }).DontCaptureContext();
            }
        }

        private SQLiteConnection _Connect() => new SQLiteConnection(mConnectionString, true);

        private Task<DatabaseUpgradeResult> _Initialize()
        {
            var upgradeEngine = DeployChanges.To
                .SQLiteDatabase(new SharedConnection(_Connect()))
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToTrace()
                .Build();

            return Task.Run(new Func<DatabaseUpgradeResult>(upgradeEngine.PerformUpgrade));
        }

        public static async Task<IPersistenceService> Create(string directory, string fileName = ".aspect.sqlite")
        {
            LogEx.For(typeof(PersistenceService))
                .Information("Creating persistence service for {Directory}\\{FileName}", directory, fileName);

            try
            {
                var dbFile = new FileInfo(Path.Combine(directory, fileName));
                if (!dbFile.Exists)
                {
                    SQLiteConnection.CreateFile(dbFile.FullName);
                    dbFile.Attributes = FileAttributes.NotContentIndexed | FileAttributes.Hidden;
                }

                var connectionString = new SQLiteConnectionStringBuilder
                {
                    DataSource = dbFile.FullName,
                    DateTimeFormat = SQLiteDateFormats.UnixEpoch,
                    BinaryGUID = true,
                    DateTimeKind = DateTimeKind.Utc,
                    ReadOnly = false
                }.ToString();

                var persistence = new PersistenceService(connectionString);
                var results = await persistence._Initialize().DontCaptureContext();

                if (!results.Successful)
                {
                    LogEx.For(typeof(PersistenceService))
                        .Error(results.Error, "Failed to migrate database {FileName}", dbFile.FullName);
                    return new NoOpPersistenceService();
                }

                return persistence;
            }
            catch (Exception ex)
            {
                LogEx.For(typeof(PersistenceService))
                    .Error(ex, "Failed to initialize SQLite persistence service.");
                return new NoOpPersistenceService();
            }
        }
    }

    public sealed class NoOpPersistenceService : IPersistenceService
    {
        public Task AddTagToFile(FileData file, long tagId) => Task.CompletedTask;

        public Task<long> CreateTag(string name) => Task.FromResult(0L);

        public Task<TagRow[]> GetAllTags() => Task.FromResult(new TagRow[0]);

        public Task<long> GetFileId(FileData file) => Task.FromResult(0L);

        public Task<long[]> GetFilesWithTag(long tagId) => Task.FromResult(new long[0]);

        public Task<string[]> GetTagsForFile(FileData file) => Task.FromResult(new string[0]);

        public Task InitializeFiles(IEnumerable<FileData> files) => Task.CompletedTask;

        public bool IsEnabled => false;

        public Task RemoveTagFromFile(FileData file, long tagId) => Task.CompletedTask;

        public Task UpdateRating(FileData file) => Task.CompletedTask;
    }
}
