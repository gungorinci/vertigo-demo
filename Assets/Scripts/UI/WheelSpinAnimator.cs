using System;
using DG.Tweening;
using UnityEngine;
using VertigoDemo.Core;

namespace VertigoDemo.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class WheelSpinAnimator : MonoBehaviour
    {
        private const string WheelTransformName = "ui_animator_wheel";
        private const string PointerName = "ui_image_pointer_value";

        [SerializeField, HideInInspector] private RectTransform wheelTransform;
        [SerializeField, HideInInspector] private RectTransform pointer;

        [SerializeField, Min(1)] private int fullTurns = 10;
        [SerializeField, Min(0.5f)] private float duration = 7f;
        [SerializeField] private Ease ease = Ease.OutQuart;
        [SerializeField, Range(0f, 0.45f)] private float maxOffsetInSlice = 0.3f;
        [SerializeField] private AudioClip tickClip;
        [SerializeField, Min(0f)] private float minTickInterval = 0.03f;

        [SerializeField, HideInInspector] private AudioSource audioSource;

        private float unwrappedAngle;
        private float lastEulerZ;
        private int lastSliceUnderPointer;
        private float lastTickTime;

        private Tween spinTween;

        public bool IsSpinning => spinTween != null && spinTween.IsActive();

        public void Spin(int targetIndex, int sliceCount, IRandomSource random, Action onComplete)
        {
            if (IsSpinning) return;

            float sliceAngle = 360f / sliceCount;
            float offset = (random.Range(-1000, 1001) / 1000f) * maxOffsetInSlice * sliceAngle;
            float targetAngle = sliceAngle * targetIndex - PointerAngle() + offset;

            float current = wheelTransform.localEulerAngles.z;
            float toTarget = Mathf.Repeat(current - targetAngle, 360f);   // clockwise distance
            float totalRotation = toTarget + fullTurns * 360f;

            float pointerAngle = PointerAngle();
            lastEulerZ = current;
            unwrappedAngle = current;
            lastSliceUnderPointer = SliceUnderPointer(pointerAngle, sliceAngle);

            spinTween = wheelTransform
                .DOLocalRotate(new Vector3(0f, 0f, -totalRotation), duration, RotateMode.LocalAxisAdd)
                .SetEase(ease)
                .SetLink(gameObject)
                .OnUpdate(() => UpdateTicks(pointerAngle, sliceAngle))
                .OnComplete(() =>
                {
                    wheelTransform.localEulerAngles = new Vector3(0f, 0f, targetAngle); // exact final angle
                    spinTween = null;
                    onComplete?.Invoke();
                });
        }

        // Degrees clockwise from the top of the wheel to the pointer.
        private float PointerAngle()
        {
            Vector2 direction = pointer.localPosition - wheelTransform.localPosition;
            return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        }

        private void UpdateTicks(float pointerAngle, float sliceAngle)
        {
            float eulerZ = wheelTransform.localEulerAngles.z;
            unwrappedAngle += Mathf.DeltaAngle(lastEulerZ, eulerZ);
            lastEulerZ = eulerZ;

            int slice = SliceUnderPointer(pointerAngle, sliceAngle);
            if (slice == lastSliceUnderPointer) return;
            lastSliceUnderPointer = slice;

            if (tickClip == null || Time.time - lastTickTime < minTickInterval) return;
            lastTickTime = Time.time;
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(tickClip);
        }

        private int SliceUnderPointer(float pointerAngle, float sliceAngle) =>
            Mathf.FloorToInt((pointerAngle + unwrappedAngle) / sliceAngle + 0.5f);
        

#if UNITY_EDITOR
        private void OnValidate()
        {
            wheelTransform = transform.FindDeep(WheelTransformName) as RectTransform;
            pointer = transform.FindDeep(PointerName) as RectTransform;
            audioSource = GetComponent<AudioSource>();

            if (wheelTransform == null || pointer == null)
                Debug.LogWarning($"{name}: wheel or pointer not found.", this);
        }
#endif
    }
}