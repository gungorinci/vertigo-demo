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
            icon.preserveAspect = true;
            amountText.text = "x" + amount;
        }

        public RectTransform IconTransform => icon.rectTransform;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            icon = transform.FindDeep(IconName)?.GetComponent<Image>();
            amountText = transform.FindDeep(AmountTextName)?.GetComponent<TMP_Text>();

            if (icon == null || amountText == null)
                Debug.LogWarning($"{name}: slice children not found.", this);
        }
#endif
    }
}