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
            var inventory = new Inventory<string>();

            inventory.Add("cash", 10);
            inventory.Add("cash", 5);

            Assert.AreEqual(15, inventory.GetAmount("cash"));
        }

        [Test]
        public void LeavingTransfersTheCollectedRewards()
        {
            var session = new GameSession<string>();
            var inventory = new Inventory<string>();
            int changes = 0;
            inventory.Changed += () => changes++;

            session.BeginSpin();
            session.CompleteSpin("cash", 7, isBomb: false);
            inventory.AddAll(session.Collected);
            session.Leave();

            Assert.AreEqual(7, inventory.GetAmount("cash"));
            Assert.AreEqual(1, changes);
        }
    }
}
