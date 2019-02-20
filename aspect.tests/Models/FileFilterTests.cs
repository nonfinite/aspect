using System;

using Aspect.Models;
using Aspect.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspect.Tests.Models
{
    [TestClass]
    public sealed class FileFilterTests
    {
        private FileFilter _CreateFilter() =>
            new FileFilter(TagService.Create(new NoOpPersistenceService()).Result, new NoOpPersistenceService());

        [TestMethod]
        public void RatingFilter()
        {
            var filter = _CreateFilter();

            Assert.IsTrue(filter.IsMatch(_WithRating(null)).Result);
            Assert.IsTrue(filter.IsMatch(_WithRating(1)).Result);
            Assert.IsTrue(filter.IsMatch(_WithRating(5)).Result);

            filter.Rating = new Rating(1);
            Assert.IsFalse(filter.IsMatch(_WithRating(null)).Result);
            Assert.IsTrue(filter.IsMatch(_WithRating(1)).Result);
            Assert.IsTrue(filter.IsMatch(_WithRating(5)).Result);

            filter.Rating = new Rating(2);
            Assert.IsFalse(filter.IsMatch(_WithRating(null)).Result);
            Assert.IsFalse(filter.IsMatch(_WithRating(1)).Result);
            Assert.IsTrue(filter.IsMatch(_WithRating(5)).Result);

            FileData _WithRating(byte? rating)
            {
                return new FileData("C:\\file", DateTime.Now, new FileSize(0))
                {
                    Rating = rating.HasValue ? new Rating(rating.Value) : (Rating?) null
                };
            }
        }

        [TestMethod]
        [DataRow(null, "file.png", true)]
        [DataRow(null, "file.jpg", true)]
        [DataRow("", "file.png", true)]
        [DataRow("", "file.jpg", true)]
        [DataRow("file.png", "file.png", true)]
        [DataRow("file.png", "file.pngs", true)]
        [DataRow("file.png", "sfile.png", true)]
        [DataRow("file.png", "sFILE.png", true)]
        [DataRow("file.png", "file.jpg", false)]
        [DataRow("file.*", "file.png", true)]
        [DataRow("file.*", "file.jpg", true)]
        [DataRow("file.*", "afile.jpg", false)]
        [DataRow("file.*", "FILE.jpg", true)]
        [DataRow("*mid*", "mid.jpg", true)]
        [DataRow("*mid*", "amid", true)]
        [DataRow("*mid*", "amida", true)]
        [DataRow("*mid*", "aMIDa", true)]
        [DataRow("*mid*", "amia", false)]
        public void TextFilter(string text, string filename, bool isMatch)
        {
            var filter = _CreateFilter();
            filter.Text = text;
            var file = new FileData("C:\\path\\" + filename, DateTime.Now, new FileSize(0));
            Assert.AreEqual(isMatch, filter.IsMatch(file));
        }
    }
}
