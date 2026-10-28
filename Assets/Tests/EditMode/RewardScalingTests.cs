using NUnit.Framework;
using VertigoDemo.Core;

namespace VertigoDemo.Tests
{
    public class RewardScalingTests
    {
        private const float GROWTH = 0.08f;

        [Test]
        public void LaterZone_GivesMoreThanFirstZone()
        {
            int first = RewardScaling.ScaleAmount(10, 1, GROWTH);
            int later = RewardScaling.ScaleAmount(10, 29, GROWTH);

            Assert.Greater(later, first);
        }

        [Test]
        public void FirstZone_KeepsTheBaseAmount()
        {
            Assert.AreEqual(10, RewardScaling.ScaleAmount(10, 1, GROWTH));
        }

        [Test]
        public void Amount_NeverDropsAsZonesGrow()
        {
            int previous = RewardScaling.ScaleAmount(10, 1, GROWTH);
            for (int zone = 2; zone <= 60; zone++)
            {
                int current = RewardScaling.ScaleAmount(10, zone, GROWTH);
                Assert.GreaterOrEqual(current, previous);
                previous = current;
            }
        }
    }
}
