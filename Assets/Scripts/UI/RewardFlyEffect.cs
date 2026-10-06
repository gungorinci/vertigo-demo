using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class RewardFlyEffect : MonoBehaviour
    {
        private const string FlyImageName = "ui_image_reward_fly_value";

        [SerializeField, HideInInspector] private Image flyImage;
        [SerializeField, Min(0.1f)] private float duration = 0.6f;
        [SerializeField] private float arcHeight = 200f;

        public void Fly(Sprite sprite, RectTransform from, Vector3 to, Action onArrived)
        {
            RectTransform rt = flyImage.rectTransform;
            rt.DOKill();

            flyImage.sprite = sprite;
            rt.position = from.position;
            rt.sizeDelta = from.rect.size;
            rt.localScale = Vector3.one;
            flyImage.gameObject.SetActive(true);

            Vector3 middle = (rt.position + to) * 0.5f + rt.parent.TransformVector(Vector3.up * arcHeight);

            DOTween.Sequence()
                .Join(rt.DOPath(new[] { middle, to }, duration, PathType.CatmullRom).SetEase(Ease.InOutQuad))
                .Join(rt.DOScale(0.6f, duration))
                .OnComplete(() =>
                {
                    flyImage.gameObject.SetActive(false);
                    onArrived?.Invoke();
                })
                .SetLink(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            flyImage = transform.FindDeepComponent<Image>(FlyImageName);
        }
#endif
    }
}
