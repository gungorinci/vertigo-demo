using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class RewardBoxView : MonoBehaviour
    {
        private const string IconName = "ui_image_reward_icon_value";
        private const string AmountTextName = "ui_text_reward_info_value";

        [SerializeField, HideInInspector] private Image icon;
        [SerializeField, HideInInspector] private TMP_Text amountText;

        public void Show(Sprite sprite, int total)
        {
            icon.sprite = sprite;
            icon.preserveAspect = true;
            amountText.text = "x" + total;

            transform.DOKill(true);
            transform.localScale = Vector3.one;
            transform.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            icon = transform.FindDeepComponent<Image>(IconName);
            amountText = transform.FindDeepComponent<TMP_Text>(AmountTextName);
        }
#endif
    }
}