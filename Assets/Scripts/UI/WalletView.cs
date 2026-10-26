using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.Data;

namespace VertigoDemo.UI
{
    public class WalletView : MonoBehaviour
    {
        private const string CONTAINER_NAME = "ui_container_wallet";

        [SerializeField] private WalletEntryView _entryPrefab;
        [SerializeField, HideInInspector] private RectTransform _container;

        private readonly Dictionary<RewardData, WalletEntryView> _entries = new Dictionary<RewardData, WalletEntryView>();

        #region Public API

        public void Show(RewardData currency, int amount)
        {
            WalletEntryView entry;
            if (!_entries.TryGetValue(currency, out entry))
            {
                entry = Instantiate(_entryPrefab, _container);
                entry.name = _entryPrefab.name;
                _entries.Add(currency, entry);
            }
            entry.Show(currency.Icon, amount);
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _container = transform.FindDeepComponent<RectTransform>(CONTAINER_NAME);
        }
#endif
    }
}
