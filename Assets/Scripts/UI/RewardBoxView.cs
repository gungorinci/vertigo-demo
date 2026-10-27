using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class RewardBoxView : MonoBehaviour
    {
        private const string IconName = "ui_image_reward_icon_value";
        private const string AmountTextName = "ui_text_reward_amount_value";
        private const string AnimatorName = "ui_animator_box_reward";

        [SerializeField, HideInInspector] private Image icon;
        [SerializeField, HideInInspector] private TMP_Text amountText;
        [SerializeField, HideInInspector] private RectTransform animator;

        public void Show(Sprite sprite, int total)
        {
            icon.sprite = sprite;
            amountText.text = "x" + total;

            animator.DOKill(true);
            animator.localScale = Vector3.one;
            animator.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            icon = transform.FindDeepComponent<Image>(IconName);
            amountText = transform.FindDeepComponent<TMP_Text>(AmountTextName);
            animator = transform.FindDeepComponent<RectTransform>(AnimatorName);
        }
#endif
    }
}