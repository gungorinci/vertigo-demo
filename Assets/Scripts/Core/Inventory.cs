using System;
using System.Collections.Generic;

namespace VertigoDemo.Core
{
    public class Inventory<TItem> : IInventory<TItem>
    {
        private readonly Dictionary<TItem, int> _items = new Dictionary<TItem, int>();
        private readonly IEventBus _bus;

        public Inventory(IEventBus bus)
        {
            _bus = bus;
        }

        public IReadOnlyDictionary<TItem, int> Items
        {
            get { return _items; }
        }

        #region Public API

        public int GetAmount(TItem item)
        {
            int amount;
            _items.TryGetValue(item, out amount);
            return amount;
        }

        public void Add(TItem item, int amount)
        {
            AddWithoutNotify(item, amount);
            NotifyChanged();
        }

        public void AddAll(IReadOnlyDictionary<TItem, int> items)
        {
            foreach (KeyValuePair<TItem, int> pair in items)
            {
                AddWithoutNotify(pair.Key, pair.Value);
            }
            NotifyChanged();
        }

        #endregion

        #region Internals

        private void AddWithoutNotify(TItem item, int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
            }
            _items[item] = GetAmount(item) + amount;
        }

        private void NotifyChanged()
        {
            _bus.Publish(new InventoryChangedEvent());
        }

        #endregion
    }
}
