using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class RewardBoxViewMono : MonoBehaviour
    {
        private const string ICON_NAME = "ui_image_reward_icon_value";
        private const string AMOUNT_TEXT_NAME = "ui_text_reward_amount_value";
        private const string ANIMATOR_NAME = "ui_animator_box_reward";

        [SerializeField, HideInInspector, FormerlySerializedAs("icon")] private Image _icon;
        [SerializeField, HideInInspector, FormerlySerializedAs("amountText")] private TMP_Text _amountText;
        [SerializeField, HideInInspector, FormerlySerializedAs("animator")] private RectTransform _animator;

        #region Public API

        public void Show(Sprite sprite, int total)
        {
            _icon.sprite = sprite;
            _amountText.text = "x" + total;

            _animator.DOKill(true);
            _animator.localScale = Vector3.one;
            _animator.DOPunchScale(Vector3.one * 0.2f, 0.3f).SetLink(gameObject);
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _icon = transform.FindDeepComponent<Image>(ICON_NAME);
            _amountText = transform.FindDeepComponent<TMP_Text>(AMOUNT_TEXT_NAME);
            _animator = transform.FindDeepComponent<RectTransform>(ANIMATOR_NAME);
        }
#endif
    }
}