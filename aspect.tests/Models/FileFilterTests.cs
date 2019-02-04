using System;

using Aspect.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspect.Tests.Models
{
    [TestClass]
    public sealed class FileFilterTests
    {
        [TestMethod]
        public void Test()
        {
            var filter = new FileFilter();

            Assert.IsTrue(filter.IsMatch(_WithRating(null)));
            Assert.IsTrue(filter.IsMatch(_WithRating(1)));
            Assert.IsTrue(filter.IsMatch(_WithRating(5)));

            filter.Rating = new Rating(1);
            Assert.IsFalse(filter.IsMatch(_WithRating(null)));
            Assert.IsTrue(filter.IsMatch(_WithRating(1)));
            Assert.IsTrue(filter.IsMatch(_WithRating(5)));

            filter.Rating = new Rating(2);
            Assert.IsFalse(filter.IsMatch(_WithRating(null)));
            Assert.IsFalse(filter.IsMatch(_WithRating(1)));
            Assert.IsTrue(filter.IsMatch(_WithRating(5)));

            FileData _WithRating(byte? rating)
            {
                return new FileData("C:\\file", DateTime.Now, new FileSize(0))
                {
                    Rating = rating.HasValue ? new Rating(rating.Value) : (Rating?) null
                };
            }
        }
    }
}
