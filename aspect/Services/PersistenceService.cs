using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Aspect.Models;

using DbUp;
using DbUp.Engine;
using DbUp.SQLite.Helpers;

namespace Aspect.Services
{
    public class PersistenceService : IDisposable
    {
        private PersistenceService(IDbConnection connection)
        {
            mConnection = connection;
        }

        private readonly IDbConnection mConnection;

        public void Dispose() => mConnection.Dispose();

        public static async Task<PersistenceService> Initialize(string directory)
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

            var connection = new SQLiteConnection(connectionString, true);
            connection.Open();

            var upgradeEngine = DeployChanges.To
                .SQLiteDatabase(new SharedConnection(connection))
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToTrace()
                .Build();

            var results = await Task.Run(new Func<DatabaseUpgradeResult>(upgradeEngine.PerformUpgrade));

            if (!results.Successful)
            {
                // TODO: handle this more elegantly
                throw new Exception($"Failed to migrate database: {dbFile.FullName}", results.Error);
            }

            return new PersistenceService(connection);
        }

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
    }
}
