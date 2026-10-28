using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class WheelSliceViewMono : MonoBehaviour
    {
        private const string ICON_NAME = "ui_image_slice_reward_value";
        private const string AMOUNT_TEXT_NAME = "ui_text_slice_amount_value";

        [SerializeField, HideInInspector, FormerlySerializedAs("icon")] private Image _icon;
        [SerializeField, HideInInspector, FormerlySerializedAs("amountText")] private TMP_Text _amountText;

        public void Show(Sprite sprite, int amount)
        {
            _icon.sprite = sprite;
            _amountText.text = "x" + amount;
        }

        public RectTransform IconTransform => _icon.rectTransform;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _icon = transform.FindDeepComponent<Image>(ICON_NAME);
            _amountText = transform.FindDeepComponent<TMP_Text>(AMOUNT_TEXT_NAME);

            if (!_icon || !_amountText)
                Debug.LogWarning($"{name}: slice children not found.", this);
        }
#endif
    }
}