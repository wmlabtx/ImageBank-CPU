using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImageBank.Tests
{
    [TestClass()]
    public class EloHelperTests
    {
        [TestMethod()]
        public void ComputeTest()
        {
            int newX, newY;
            EloHelper.Compute(2000, 2000, 1, 0, out newX, out newY);
            Assert.AreEqual(newX, 2020);
            Assert.AreEqual(newY, 1980);

            EloHelper.Compute(2000, 2000, 0, 1, out newX, out newY);
            Assert.AreEqual(newX, 1980);
            Assert.AreEqual(newY, 2020);

            EloHelper.Compute(2200, 1800, 1, 0, out newX, out newY);
            Assert.AreEqual(newX, 2204);
            Assert.AreEqual(newY, 1796);

            EloHelper.Compute(2200, 1800, 0, 1, out newX, out newY);
            Assert.AreEqual(newX, 2164);
            Assert.AreEqual(newY, 1836);
       }
    }
}