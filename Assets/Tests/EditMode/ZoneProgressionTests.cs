using NUnit.Framework;
using VertigoDemo.Core;

namespace VertigoDemo.Tests
{
    public class ZoneProgressionTests
    {
        private const string BRONZE = "bronze";
        private const string SILVER = "silver";
        private const string GOLD = "gold";

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

        private ZoneSchedule<TestZoneType> _schedule;
        private GameSession<string> _session;

        [SetUp]
        public void SetUp()
        {
            _schedule = new ZoneSchedule<TestZoneType>(new[]
            {
                new TestZoneType(BRONZE, 1, true),
                new TestZoneType(SILVER, 5, false),
                new TestZoneType(GOLD, 30, false)
            });
            _session = new GameSession<string>(new EventBus());
        }

        [TestCase(1, BRONZE, true, false)]
        [TestCase(2, BRONZE, true, true)]
        [TestCase(5, SILVER, false, true)]
        [TestCase(6, BRONZE, true, true)]
        [TestCase(30, GOLD, false, true)]
        [TestCase(60, GOLD, false, true)]
        [TestCase(65, SILVER, false, true)]
        public void Zone_ShowsExpectedWheel(int zone, string expectedWheel, bool hasBomb, bool canLeave)
        {
            WinUntil(zone);

            Assert.AreEqual(zone, _session.Zone);
            Assert.AreEqual(expectedWheel, _schedule.For(_session.Zone).Name);
            Assert.AreEqual(hasBomb, _schedule.For(_session.Zone).HasBomb);
            Assert.AreEqual(canLeave, _session.CanLeave);
        }

        [TestCase(1, 5)]
        [TestCase(5, 10)]
        [TestCase(26, 30)]
        public void NextSpecialZone_IsTheNextSilverOrGoldZone(int zone, int expectedZone)
        {
            int nextZone;

            Assert.IsTrue(_schedule.TryFindNextSpecialZone(zone, out nextZone));
            Assert.AreEqual(expectedZone, nextZone);
        }

        [Test]
        public void NewZoneType_NeedsNoCodeChange()
        {
            var withPlatinum = new ZoneSchedule<TestZoneType>(new[]
            {
                new TestZoneType(BRONZE, 1, true),
                new TestZoneType(SILVER, 5, false),
                new TestZoneType("platinum", 50, false)
            });

            Assert.AreEqual("platinum", withPlatinum.For(50).Name);
            Assert.AreEqual(SILVER, withPlatinum.For(55).Name);
        }

        [Test]
        public void Schedule_NeedsATypeForEveryZone()
        {
            Assert.Throws<System.ArgumentException>(() =>
                new ZoneSchedule<TestZoneType>(new[] { new TestZoneType(SILVER, 5, false) }));
        }

        [Test]
        public void Rewards_AccumulateAcrossZones()
        {
            WinUntil(5);

            Assert.AreEqual(4, _session.Collected["cash"]);
        }

        [Test]
        public void Bomb_LosesEverything_AndRestartGoesBackToZoneOne()
        {
            WinUntil(7);

            _session.BeginSpin();
            _session.CompleteSpin("bomb", 1, isBomb: true);

            Assert.AreEqual(SessionState.Lost, _session.State);
            Assert.IsEmpty(_session.Collected);

            _session.Restart();

            Assert.AreEqual(1, _session.Zone);
            Assert.AreEqual(SessionState.Idle, _session.State);
        }

        [Test]
        public void Leave_IsRejectedBeforeAnyReward()
        {
            Assert.IsFalse(_session.CanLeave);
            Assert.Throws<System.InvalidOperationException>(() => _session.Leave());
        }

        [Test]
        public void Leave_IsAllowedAfterASpinOnNormalZone()
        {
            WinUntil(3);

            Assert.AreEqual(BRONZE, _schedule.For(_session.Zone).Name);
            Assert.IsTrue(_session.CanLeave);

            _session.Leave();

            Assert.AreEqual(SessionState.Left, _session.State);
        }

        // Plays winning spins (1 cash each) until the session reaches the given zone.
        private void WinUntil(int zone)
        {
            while (_session.Zone < zone)
            {
                _session.BeginSpin();
                _session.CompleteSpin("cash", 1, isBomb: false);
            }
        }
    }
}
