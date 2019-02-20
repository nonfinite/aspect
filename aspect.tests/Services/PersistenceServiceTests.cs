using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Aspect.Models;
using Aspect.Services;
using Aspect.Utility;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspect.Tests.Services
{
    [TestClass]
    public class PersistenceServiceTests
    {
        private static string mDirectory;

        private Task<IPersistenceService> _CreatePersistenceService([CallerMemberName] string testName = null) =>
            PersistenceService.Create(mDirectory, $"{testName}.sqlite");

        [ClassCleanup]
        public static void Cleanup()
        {
            try
            {
                Directory.Delete(mDirectory, true);
            }
            catch (Exception ex)
            {
                LogEx.For(typeof(PersistenceServiceTests))
                    .Error(ex, "Failed to clean up {Directory}", mDirectory);
            }
        }

        [TestMethod]
        public async Task FileRatingsPersist()
        {
            var file = new FileData("C:\\file.png", DateTime.Now, new FileSize());
            var svc = await _CreatePersistenceService();
            await svc.InitializeFiles(new[] {file});

            Assert.AreEqual(null, file.Rating, "Files should start unrated");
            file.Rating = new Rating(3);
            await svc.UpdateRating(file);

            svc = await _CreatePersistenceService();
            file.Rating = null;

            await svc.InitializeFiles(new[] {file});
            Assert.AreEqual(new Rating(3), file.Rating, "Ratings should persist between services");
        }

        [TestMethod]
        public async Task GetFileId()
        {
            var file = new FileData("C:\\file.png", DateTime.Now, new FileSize());
            var svc = await _CreatePersistenceService();

            file.Id = null;
            var originalId = await svc.GetFileId(file);
            Assert.AreEqual(originalId, file.Id, "file.Id should be set when getting the id");

            var otherFile = new FileData("C:\\other.png", DateTime.Now, new FileSize());
            var otherId = await svc.GetFileId(otherFile);
            Assert.AreNotEqual(originalId, otherId, "Other files should get a different id");

            file.Id = null;
            var newId = await svc.GetFileId(file);
            Assert.AreEqual(originalId, newId, "The same file should always return the same id");
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            mDirectory = Path.Combine(Path.GetTempPath(), $"aspect.test.{Guid.NewGuid():N}");
            Directory.CreateDirectory(mDirectory);
            LogEx.For(typeof(PersistenceService))
                .Information("Created {Directory} for test files", mDirectory);
        }
    }
}
