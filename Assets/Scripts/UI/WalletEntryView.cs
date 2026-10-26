using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class WalletEntryView : MonoBehaviour
    {
        private const string ICON_NAME = "ui_image_wallet_icon_value";
        private const string AMOUNT_TEXT_NAME = "ui_text_wallet_amount_value";

        [SerializeField, HideInInspector] private Image _icon;
        [SerializeField, HideInInspector] private TMP_Text _amountText;

        #region Public API

        public void Show(Sprite sprite, int amount)
        {
            _icon.sprite = sprite;
            _amountText.text = amount.ToString();
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _icon = transform.FindDeepComponent<Image>(ICON_NAME);
            _amountText = transform.FindDeepComponent<TMP_Text>(AMOUNT_TEXT_NAME);
        }
#endif
    }
}
