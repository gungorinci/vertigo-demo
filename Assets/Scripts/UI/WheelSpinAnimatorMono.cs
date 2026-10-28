using System;
using DG.Tweening;
using UnityEngine;
using VertigoDemo.Core;

namespace VertigoDemo.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class WheelSpinAnimatorMono : MonoBehaviour
    {
        private const string WheelTransformName = "ui_animator_wheel";
        private const string PointerName = "ui_image_pointer_value";
        private const float PITCH_VARIATION = 0.05f;
        private const int PITCH_STEPS = 1000;

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
        private IRandomSource _random;
        private float _pointerAngle;
        private float _sliceAngle;
        private float _targetAngle;
        private Action _onComplete;

        public bool IsSpinning => spinTween != null && spinTween.IsActive();

        public void Spin(int targetIndex, int sliceCount, IRandomSource random, Action onComplete)
        {
            if (IsSpinning)
            {
                throw new InvalidOperationException("The wheel is already spinning.");
            }
            _random = random;

            _onComplete = onComplete;
            _sliceAngle = 360f / sliceCount;
            _pointerAngle = PointerAngle();
            float offset = (random.Range(-1000, 1001) / 1000f) * maxOffsetInSlice * _sliceAngle;
            _targetAngle = _sliceAngle * targetIndex - _pointerAngle + offset;

            float current = wheelTransform.localEulerAngles.z;
            float toTarget = Mathf.Repeat(current - _targetAngle, 360f);   // clockwise distance
            float totalRotation = toTarget + fullTurns * 360f;

            lastEulerZ = current;
            unwrappedAngle = current;
            lastSliceUnderPointer = SliceUnderPointer(_pointerAngle, _sliceAngle);

            spinTween = wheelTransform
                .DOLocalRotate(new Vector3(0f, 0f, -totalRotation), duration, RotateMode.LocalAxisAdd)
                .SetEase(ease)
                .SetLink(gameObject)
                .OnUpdate(UpdateTicks)
                .OnComplete(OnSpinComplete);
        }

        private void OnSpinComplete()
        {
            wheelTransform.localEulerAngles = new Vector3(0f, 0f, _targetAngle); // exact final angle
            spinTween = null;
            if (_onComplete != null)
            {
                _onComplete();
            }
        }

        // Degrees clockwise from the top of the wheel to the pointer.
        private float PointerAngle()
        {
            Vector2 direction = pointer.localPosition - wheelTransform.localPosition;
            return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        }

        private void UpdateTicks()
        {
            float eulerZ = wheelTransform.localEulerAngles.z;
            unwrappedAngle += Mathf.DeltaAngle(lastEulerZ, eulerZ);
            lastEulerZ = eulerZ;

            int slice = SliceUnderPointer(_pointerAngle, _sliceAngle);
            if (slice == lastSliceUnderPointer) return;
            lastSliceUnderPointer = slice;

            if (!tickClip || Time.time - lastTickTime < minTickInterval) return;
            lastTickTime = Time.time;
            audioSource.pitch = 1f + PITCH_VARIATION * (2f * _random.Range(0, PITCH_STEPS + 1) / PITCH_STEPS - 1f);
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

            if (!wheelTransform || !pointer)
                Debug.LogWarning($"{name}: wheel or pointer not found.", this);
        }
#endif
    }
}