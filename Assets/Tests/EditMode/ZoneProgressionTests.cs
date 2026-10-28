using NUnit.Framework;
using VertigoDemo.Core;

namespace VertigoDemo.Tests
{
    public class ZoneProgressionTests
    {
        private const string Bronze = "bronze";
        private const string Silver = "silver";
        private const string Gold = "gold";

        private class TestZoneType : IZoneType
        {
            public TestZoneType(string name, int interval, bool hasBomb)
            {
                Name = name;
                Interval = interval;
                HasBomb = hasBomb;
            }

            public string Name { get; }
            public int Interval { get; }
            public bool HasBomb { get; }
        }

        private ZoneSchedule<TestZoneType> schedule;
        private GameSession<string> session;

        [SetUp]
        public void SetUp()
        {
            schedule = new ZoneSchedule<TestZoneType>(new[]
            {
                new TestZoneType(Bronze, 1, true),
                new TestZoneType(Silver, 5, false),
                new TestZoneType(Gold, 30, false)
            });
            session = new GameSession<string>(new EventBus());
        }

        [TestCase(1, Bronze, true, false)]
        [TestCase(2, Bronze, true, true)]
        [TestCase(5, Silver, false, true)]
        [TestCase(6, Bronze, true, true)]
        [TestCase(30, Gold, false, true)]
        [TestCase(60, Gold, false, true)]
        [TestCase(65, Silver, false, true)]
        public void Zone_ShowsExpectedWheel(int zone, string expectedWheel, bool hasBomb, bool canLeave)
        {
            WinUntil(zone);

            Assert.AreEqual(zone, session.Zone);
            Assert.AreEqual(expectedWheel, schedule.For(session.Zone).Name);
            Assert.AreEqual(hasBomb, schedule.For(session.Zone).HasBomb);
            Assert.AreEqual(canLeave, session.CanLeave);
        }

        [TestCase(1, 5)]
        [TestCase(5, 10)]
        [TestCase(26, 30)]
        public void NextSpecialZone_IsTheNextSilverOrGoldZone(int zone, int expectedZone)
        {
            int nextZone;

            Assert.IsTrue(schedule.TryFindNextSpecialZone(zone, out nextZone));
            Assert.AreEqual(expectedZone, nextZone);
        }

        [Test]
        public void NewZoneType_NeedsNoCodeChange()
        {
            var withPlatinum = new ZoneSchedule<TestZoneType>(new[]
            {
                new TestZoneType(Bronze, 1, true),
                new TestZoneType(Silver, 5, false),
                new TestZoneType("platinum", 50, false)
            });

            Assert.AreEqual("platinum", withPlatinum.For(50).Name);
            Assert.AreEqual(Silver, withPlatinum.For(55).Name);
        }

        [Test]
        public void Schedule_NeedsATypeForEveryZone()
        {
            Assert.Throws<System.ArgumentException>(() =>
                new ZoneSchedule<TestZoneType>(new[] { new TestZoneType(Silver, 5, false) }));
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
        public void Leave_IsRejectedBeforeAnyReward()
        {
            Assert.IsFalse(session.CanLeave);
            Assert.Throws<System.InvalidOperationException>(() => session.Leave());
        }

        [Test]
        public void Leave_IsAllowedAfterASpinOnNormalZone()
        {
            WinUntil(3);

            Assert.AreEqual(Bronze, schedule.For(session.Zone).Name);
            Assert.IsTrue(session.CanLeave);

            session.Leave();

            Assert.AreEqual(SessionState.Left, session.State);
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
