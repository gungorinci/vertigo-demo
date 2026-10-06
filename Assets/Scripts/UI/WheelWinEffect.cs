using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class WheelWinEffect : MonoBehaviour
    {
        private const string GlowName = "ui_image_win_glow";
        private const string FlashName = "ui_image_win_flash";

        [SerializeField, HideInInspector] private Image glow;
        [SerializeField, HideInInspector] private Image flash;
        [SerializeField, HideInInspector] private WheelSliceView[] slices;

        [SerializeField, Min(0.05f)] private float pulseDuration = 0.4f;
        [SerializeField, Min(0.05f)] private float flashDuration = 0.5f;

        private void Awake() => Hide();

        public void Play(int sliceIndex)
        {
            Hide();
            RectTransform icon = slices[sliceIndex].IconTransform;
            PlaceAt(glow.rectTransform, icon, behindIcon: true);
            PlaceAt(flash.rectTransform, icon, behindIcon: false);

            // Glow: pops in, then pulses until hidden.
            glow.gameObject.SetActive(true);
            glow.rectTransform.localScale = Vector3.zero;
            glow.rectTransform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetLink(glow.gameObject)
                .OnComplete(() => glow.rectTransform
                    .DOScale(1.15f, pulseDuration).SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo).SetLink(glow.gameObject));

            // Flash: grows and fades out once.
            flash.gameObject.SetActive(true);
            flash.rectTransform.localScale = Vector3.one * 0.3f;
            Color c = flash.color;
            flash.color = new Color(c.r, c.g, c.b, 1f);
            flash.rectTransform.DOScale(1.4f, flashDuration).SetEase(Ease.OutQuad).SetLink(flash.gameObject);
            flash.DOFade(0f, flashDuration).SetLink(flash.gameObject);
        }

        public void Hide()
        {
            glow.rectTransform.DOKill();
            flash.rectTransform.DOKill();
            flash.DOKill();
            glow.gameObject.SetActive(false);
            flash.gameObject.SetActive(false);
        }

        private static void PlaceAt(RectTransform effect, RectTransform icon, bool behindIcon)
        {
            effect.SetParent(icon.parent, false);
            effect.anchoredPosition = icon.anchoredPosition;
            effect.localRotation = Quaternion.identity;

            if (behindIcon) effect.SetSiblingIndex(icon.GetSiblingIndex());
            else effect.SetAsLastSibling();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            glow = transform.FindDeepComponent<Image>(GlowName);
            flash = transform.FindDeepComponent<Image>(FlashName);
            slices = GetComponentsInChildren<WheelSliceView>(true);
        }
#endif
    }
}