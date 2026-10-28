using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class RewardFlyEffectMono : MonoBehaviour
    {
        private const string FLY_IMAGE_NAME = "ui_image_reward_fly_value";
        private const string FLY_ANIMATOR_NAME = "ui_animator_reward_fly";

        [SerializeField, HideInInspector, FormerlySerializedAs("flyImage")] private Image _flyImage;
        [SerializeField, HideInInspector, FormerlySerializedAs("flyAnimator")] private RectTransform _flyAnimator;
        [SerializeField, Min(0.1f), FormerlySerializedAs("duration")] private float _duration = 0.6f;
        [SerializeField, FormerlySerializedAs("arcHeight")] private float _arcHeight = 200f;

        private Action _onArrived;

        public void Fly(Sprite sprite, RectTransform from, Vector3 to, Action onArrived)
        {
            RectTransform rt = _flyAnimator;
            rt.DOKill();
            _onArrived = onArrived;

            _flyImage.sprite = sprite;
            rt.position = from.position;
            rt.sizeDelta = from.rect.size;
            rt.localScale = Vector3.one;
            rt.gameObject.SetActive(true);

            Vector3 middle = (rt.position + to) * 0.5f + rt.parent.TransformVector(Vector3.up * _arcHeight);

            DOTween.Sequence()
                .Join(rt.DOPath(new[] { middle, to }, _duration, PathType.CatmullRom).SetEase(Ease.InOutQuad))
                .Join(rt.DOScale(0.6f, _duration))
                .OnComplete(OnFlyComplete)
                .SetLink(gameObject);
        }

        private void OnFlyComplete()
        {
            _flyAnimator.gameObject.SetActive(false);
            if (_onArrived != null)
            {
                _onArrived();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _flyImage = transform.FindDeepComponent<Image>(FLY_IMAGE_NAME);
            _flyAnimator = transform.FindDeepComponent<RectTransform>(FLY_ANIMATOR_NAME);
        }
#endif
    }
}
