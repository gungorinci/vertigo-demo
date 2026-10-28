using NUnit.Framework;
using VertigoDemo.Core;

namespace VertigoDemo.Tests
{
    public class EventBusTests
    {
        private EventBus _bus;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
        }

        [Test]
        public void Publish_ReachesSubscribersOfThatType()
        {
            int receivedZone = 0;
            _bus.Subscribe<ZoneChangedEvent>(evt => receivedZone = evt.Zone);

            _bus.Publish(new ZoneChangedEvent(7));

            Assert.AreEqual(7, receivedZone);
        }

        [Test]
        public void Publish_DoesNotReachSubscribersOfOtherTypes()
        {
            bool called = false;
            _bus.Subscribe<RewardsClearedEvent>(evt => called = true);

            _bus.Publish(new ZoneChangedEvent(1));

            Assert.IsFalse(called);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            int calls = 0;
            System.Action<InventoryChangedEvent> handler = evt => calls++;
            _bus.Subscribe(handler);
            _bus.Unsubscribe(handler);

            _bus.Publish(new InventoryChangedEvent());

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Session_PublishesZoneAndStateChanges()
        {
            var session = new GameSession<string>(_bus);
            int zone = 0;
            SessionState lastState = SessionState.Spinning;
            _bus.Subscribe<ZoneChangedEvent>(evt => zone = evt.Zone);
            _bus.Subscribe<SessionStateChangedEvent>(evt => lastState = evt.State);

            session.BeginSpin();
            session.CompleteSpin("cash", 5, isBomb: false);

            Assert.AreEqual(2, zone);
            Assert.AreEqual(SessionState.Idle, lastState);
        }
    }
}
