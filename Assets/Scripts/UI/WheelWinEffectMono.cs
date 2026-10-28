using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class WheelWinEffectMono : MonoBehaviour
    {
        private const string GLOW_NAME = "ui_image_win_glow";
        private const string FLASH_NAME = "ui_image_win_flash";

        [SerializeField, HideInInspector, FormerlySerializedAs("glow")] private Image _glow;
        [SerializeField, HideInInspector, FormerlySerializedAs("flash")] private Image _flash;
        [SerializeField, HideInInspector, FormerlySerializedAs("slices")] private WheelSliceViewMono[] _slices;

        [SerializeField, Min(0.05f), FormerlySerializedAs("pulseDuration")] private float _pulseDuration = 0.4f;
        [SerializeField, Min(0.05f), FormerlySerializedAs("flashDuration")] private float _flashDuration = 0.5f;

        #region Unity

        private void Awake()
        {
            Hide();
        }

        #endregion

        #region Public API

        public void Play(int sliceIndex)
        {
            Hide();
            RectTransform icon = _slices[sliceIndex].IconTransform;
            PlaceAt(_glow.rectTransform, icon, isBehindIcon: true);
            PlaceAt(_flash.rectTransform, icon, isBehindIcon: false);

            // Glow: pops in, then pulses until hidden.
            _glow.gameObject.SetActive(true);
            _glow.rectTransform.localScale = Vector3.zero;
            _glow.rectTransform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetLink(_glow.gameObject)
                .OnComplete(StartGlowPulse);

            // Flash: grows and fades out once.
            _flash.gameObject.SetActive(true);
            _flash.rectTransform.localScale = Vector3.one * 0.3f;
            Color c = _flash.color;
            _flash.color = new Color(c.r, c.g, c.b, 1f);
            _flash.rectTransform.DOScale(1.4f, _flashDuration).SetEase(Ease.OutQuad).SetLink(_flash.gameObject);
            _flash.DOFade(0f, _flashDuration).SetLink(_flash.gameObject);
        }

        public void Hide()
        {
            _glow.rectTransform.DOKill();
            _flash.rectTransform.DOKill();
            _flash.DOKill();
            _glow.gameObject.SetActive(false);
            _flash.gameObject.SetActive(false);
        }

        #endregion

        #region Effects

        private void StartGlowPulse()
        {
            _glow.rectTransform.DOScale(1.15f, _pulseDuration).SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo).SetLink(_glow.gameObject);
        }

        private static void PlaceAt(RectTransform effect, RectTransform icon, bool isBehindIcon)
        {
            effect.SetParent(icon.parent, false);
            effect.anchoredPosition = icon.anchoredPosition;
            effect.localRotation = Quaternion.identity;

            if (isBehindIcon)
            {
                effect.SetSiblingIndex(icon.GetSiblingIndex());
            }
            else
            {
                effect.SetAsLastSibling();
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _glow = transform.FindDeepComponent<Image>(GLOW_NAME);
            _flash = transform.FindDeepComponent<Image>(FLASH_NAME);
            _slices = GetComponentsInChildren<WheelSliceViewMono>(true);
        }
#endif
    }
}
