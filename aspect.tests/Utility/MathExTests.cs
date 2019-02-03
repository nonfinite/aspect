using Aspect.Utility;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspect.Tests.Utility
{
    [TestClass]
    public class MathExTests
    {
        [TestMethod]
        [DataRow(-1, 1, 5, 1)]
        [DataRow(1, 1, 5, 1)]
        [DataRow(2, 1, 5, 2)]
        [DataRow(4, 1, 5, 4)]
        [DataRow(5, 1, 5, 5)]
        [DataRow(6, 1, 5, 5)]
        public void Clamp(int value, int lower, int upper, int expected) =>
            Assert.AreEqual(expected, MathEx.Clamp(value, lower, upper));
    }
}
