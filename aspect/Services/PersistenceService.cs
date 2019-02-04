using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Aspect.Models;

namespace Aspect.Services
{
    public class PersistenceService
    {
        public static Task<PersistenceService> Initialize(string directory) =>
            Task.FromResult(new PersistenceService());

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
