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
        Task InitializeFiles(IEnumerable<FileData> files);

        Task UpdateRating(FileData file);
    }

    public class PersistenceService : IPersistenceService
    {
        private PersistenceService(string connectionString)
        {
            mConnectionString = connectionString;
        }

        private readonly string mConnectionString;


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
                        file.Rating = row.Rating.HasValue ? new Rating(row.Rating.Value) : (Rating?) null;
                    }
                }
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

        public static async Task<PersistenceService> Create(string directory)
        {
            var dbFile = new FileInfo(Path.Combine(directory, ".aspect.sqlite"));
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
                // TODO: handle this more elegantly
                throw new Exception($"Failed to migrate database: {dbFile.FullName}", results.Error);
            }

            return persistence;
        }
    }
}
