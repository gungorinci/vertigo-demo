using System.Collections.Generic;

namespace VertigoDemo.Core
{
    public interface IInventory<TItem>
    {
        IReadOnlyDictionary<TItem, int> Items { get; }

        int GetAmount(TItem item);

        void Add(TItem item, int amount);

        void AddAll(IReadOnlyDictionary<TItem, int> items);
    }
}
