using Coditate.TestSupport;
using NUnit.Framework;

namespace Coditate.Common.Util
{
    [TestFixture]
    public class MathUtilsTest
    {
        [Test]
        public void Constrain()
        {
            int value = RandomData.Int();
            int min = RandomData.Int();
            int max = RandomData.Generator.Next(min, int.MaxValue);

            int constrained = MathUtils.Constrain(value, min, max);

            Assert.GreaterOrEqual(constrained, min);
            Assert.LessOrEqual(constrained, max);

            if (value <= max && value >= min)
            {
                Assert.AreEqual(value, constrained);
            }
        }

        [Test]
        public void Constrain_Infinite()
        {
            double value = MathUtils.Constrain(double.PositiveInfinity, 0, 100);
            Assert.AreEqual(100, value);

            value = MathUtils.Constrain(double.NegativeInfinity, 0, 100);
            Assert.AreEqual(0, value);
        }

        [Test]
        public void ToPercent_NegativeDenominator()
        {
            int percent = MathUtils.ToPercent(1, 0, true);
            Assert.AreEqual(100, percent);
        }

        [Test]
        public void ToPercent_Over100()
        {
            int percent = MathUtils.ToPercent(2, 1, true);
            Assert.AreEqual(100, percent);
        }

        [Test]
        public void ToPercent_UnderZero()
        {
            int percent = MathUtils.ToPercent(1, -1, true);
            Assert.AreEqual(0, percent);
        }
    }
}