using NUnit.Framework;
using VertigoDemo.Core;

namespace VertigoDemo.Tests
{
    public class ZoneProgressionTests
    {
        private const string Bronze = "bronze";
        private const string Silver = "silver";
        private const string Gold = "gold";

        private WheelSelector<string> wheels;
        private GameSession<string> session;

        [SetUp]
        public void SetUp()
        {
            wheels = new WheelSelector<string>(Bronze, Silver, Gold);
            session = new GameSession<string>();
        }

        [TestCase(1, Bronze, true, false)]
        [TestCase(2, Bronze, true, false)]
        [TestCase(5, Silver, false, true)]
        [TestCase(6, Bronze, true, false)]
        [TestCase(30, Gold, false, true)]
        [TestCase(60, Gold, false, true)]
        [TestCase(65, Silver, false, true)]
        public void Zone_ShowsExpectedWheel(int zone, string expectedWheel, bool hasBomb, bool canLeave)
        {
            WinUntil(zone);

            Assert.AreEqual(zone, session.Zone);
            Assert.AreEqual(expectedWheel, wheels.For(session.ZoneType));
            Assert.AreEqual(hasBomb, ZoneRules.HasBomb(session.ZoneType));
            Assert.AreEqual(canLeave, session.CanLeave);
        }

        [Test]
        public void Rewards_AccumulateAcrossZones()
        {
            WinUntil(5);

            Assert.AreEqual(4, session.Collected["cash"]);
        }

        [Test]
        public void Bomb_LosesEverything_AndRestartGoesBackToZoneOne()
        {
            WinUntil(7);

            session.BeginSpin();
            session.CompleteSpin("bomb", 1, isBomb: true);

            Assert.AreEqual(SessionState.Lost, session.State);
            Assert.IsEmpty(session.Collected);

            session.Restart();

            Assert.AreEqual(1, session.Zone);
            Assert.AreEqual(SessionState.Idle, session.State);
        }

        [Test]
        public void Leave_IsRejectedOnNormalZone()
        {
            WinUntil(3);

            Assert.IsFalse(session.CanLeave);
            Assert.Throws<System.InvalidOperationException>(() => session.Leave());
        }

        // Plays winning spins (1 cash each) until the session reaches the given zone.
        private void WinUntil(int zone)
        {
            while (session.Zone < zone)
            {
                session.BeginSpin();
                session.CompleteSpin("cash", 1, isBomb: false);
            }
        }
    }
}
