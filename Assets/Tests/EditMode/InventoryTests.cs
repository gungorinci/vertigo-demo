using System.Collections.Generic;
using NUnit.Framework;
using VertigoDemo.Core;

namespace VertigoDemo.Tests
{
    public class InventoryTests
    {
        [Test]
        public void AddingTheSameItem_AddsUpItsAmount()
        {
            var inventory = new Inventory<string>(new EventBus());

            inventory.Add("cash", 10);
            inventory.Add("cash", 5);

            Assert.AreEqual(15, inventory.GetAmount("cash"));
        }

        [Test]
        public void LeavingTransfersTheCollectedRewards()
        {
            var bus = new EventBus();
            var session = new GameSession<string>(bus);
            var inventory = new Inventory<string>(bus);
            int changes = 0;
            bus.Subscribe<InventoryChangedEvent>(evt => changes++);

            session.BeginSpin();
            session.CompleteSpin("cash", 7, isBomb: false);
            inventory.AddAll(session.Collected);
            session.Leave();

            Assert.AreEqual(7, inventory.GetAmount("cash"));
            Assert.AreEqual(1, changes);
        }
    }
}
