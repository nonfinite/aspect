using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Aspect.Models;
using Aspect.Utility;

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


        public Task InitializeFiles(IEnumerable<FileData> files)
        {
            Debug.WriteLine("TODO: initialize files");
            return Task.CompletedTask;
        }

        public Task UpdateRating(FileData file)
        {
            Debug.WriteLine("TODO: update file rating");
            return Task.CompletedTask;
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
