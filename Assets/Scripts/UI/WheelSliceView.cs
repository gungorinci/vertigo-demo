using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class WheelSliceView : MonoBehaviour
    {
        private const string IconName = "ui_image_slice_reward_value";
        private const string AmountTextName = "ui_text_slice_amount_value";

        [SerializeField, HideInInspector] private Image icon;
        [SerializeField, HideInInspector] private TMP_Text amountText;

        public void Show(Sprite sprite, int amount)
        {
            icon.sprite = sprite;
            amountText.text = "x" + amount;
        }

        public RectTransform IconTransform => icon.rectTransform;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            icon = transform.FindDeepComponent<Image>(IconName);
            amountText = transform.FindDeepComponent<TMP_Text>(AmountTextName);

            if (!icon || !amountText)
                Debug.LogWarning($"{name}: slice children not found.", this);
        }
#endif
    }
}