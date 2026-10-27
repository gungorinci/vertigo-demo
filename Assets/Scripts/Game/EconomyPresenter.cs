using System;
using System.Collections.Generic;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class EconomyPresenter : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly GameSession<RewardData> _session;
        private readonly ISaveStore _saveStore;
        private readonly WalletView _walletView;
        private readonly IInventory<RewardData> _inventory;
        private readonly List<RewardData> _currencies = new List<RewardData>();

        public EconomyPresenter(
            IEventBus bus,
            GameSession<RewardData> session,
            ISaveStore saveStore,
            WalletView walletView,
            IReadOnlyList<RewardData> catalog)
        {
            _bus = bus;
            _session = session;
            _saveStore = saveStore;
            _walletView = walletView;

            foreach (RewardData reward in catalog)
            {
                if (reward.IsCurrency)
                {
                    _currencies.Add(reward);
                }
            }

            _inventory = new Inventory<RewardData>(bus);
            _inventory.AddAll(InventorySave.Read(_saveStore.Load(), catalog));
            ShowWallet();

            _bus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            _bus.Subscribe<SessionStateChangedEvent>(OnStateChanged);
        }

        #region Public API

        public void Dispose()
        {
            _bus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            _bus.Unsubscribe<SessionStateChangedEvent>(OnStateChanged);
        }

        #endregion

        #region Handlers

        private void OnStateChanged(SessionStateChangedEvent evt)
        {
            if (evt.State == SessionState.Left)
            {
                _inventory.AddAll(_session.Collected);
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            _saveStore.Save(InventorySave.Write(_inventory.Items));
            ShowWallet();
        }

        #endregion

        #region Wallet

        private void ShowWallet()
        {
            foreach (RewardData currency in _currencies)
            {
                _walletView.Show(currency, _inventory.GetAmount(currency));
            }
        }

        #endregion
    }
}
