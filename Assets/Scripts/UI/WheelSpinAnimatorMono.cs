using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using VertigoDemo.Core;

namespace VertigoDemo.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class WheelSpinAnimatorMono : MonoBehaviour
    {
        private const string WHEEL_TRANSFORM_NAME = "ui_animator_wheel";
        private const string POINTER_NAME = "ui_image_pointer_value";

        [SerializeField, HideInInspector, FormerlySerializedAs("wheelTransform")] private RectTransform _wheelTransform;
        [SerializeField, HideInInspector, FormerlySerializedAs("pointer")] private RectTransform _pointer;

        [SerializeField, Min(1), FormerlySerializedAs("fullTurns")] private int _fullTurns = 10;
        [SerializeField, Min(0.5f), FormerlySerializedAs("duration")] private float _duration = 7f;
        [SerializeField, FormerlySerializedAs("ease")] private Ease _ease = Ease.OutQuart;
        [SerializeField, Range(0f, 0.45f), FormerlySerializedAs("maxOffsetInSlice")] private float _maxOffsetInSlice = 0.3f;
        [SerializeField, FormerlySerializedAs("tickClip")] private AudioClip _tickClip;
        [SerializeField, Min(0f), FormerlySerializedAs("minTickInterval")] private float _minTickInterval = 0.03f;

        [SerializeField, HideInInspector, FormerlySerializedAs("audioSource")] private AudioSource _audioSource;

        private float _unwrappedAngle;
        private float _lastEulerZ;
        private int _lastSliceUnderPointer;
        private float _lastTickTime;

        private Tween _spinTween;
        private IRandomSource _random;
        private float _pointerAngle;
        private float _sliceAngle;
        private float _targetAngle;
        private Action _onComplete;

        public bool IsSpinning
        {
            get { return _spinTween != null && _spinTween.IsActive(); }
        }

        #region Public API

        public void Spin(int targetIndex, int sliceCount, IRandomSource random, Action onComplete)
        {
            if (IsSpinning)
            {
                throw new InvalidOperationException("The wheel is already spinning.");
            }
            _random = random;

            _onComplete = onComplete;
            _sliceAngle = UiEffects.DEGREES_PER_TURN / sliceCount;
            _pointerAngle = PointerAngle();
            int offsetStep = random.Range(-UiEffects.SPIN_OFFSET_STEPS, UiEffects.SPIN_OFFSET_STEPS + 1);
            float offset = (float)offsetStep / UiEffects.SPIN_OFFSET_STEPS * _maxOffsetInSlice * _sliceAngle;
            _targetAngle = _sliceAngle * targetIndex - _pointerAngle + offset;

            float current = _wheelTransform.localEulerAngles.z;
            float toTarget = Mathf.Repeat(current - _targetAngle, UiEffects.DEGREES_PER_TURN);   // clockwise distance
            float totalRotation = toTarget + _fullTurns * UiEffects.DEGREES_PER_TURN;

            _lastEulerZ = current;
            _unwrappedAngle = current;
            _lastSliceUnderPointer = SliceUnderPointer(_pointerAngle, _sliceAngle);

            _spinTween = _wheelTransform
                .DOLocalRotate(new Vector3(0f, 0f, -totalRotation), _duration, RotateMode.LocalAxisAdd)
                .SetEase(_ease)
                .SetLink(gameObject)
                .OnUpdate(UpdateTicks)
                .OnComplete(OnSpinComplete);
        }

        #endregion

        #region Tween callbacks

        private void OnSpinComplete()
        {
            _wheelTransform.localEulerAngles = new Vector3(0f, 0f, _targetAngle); // exact final angle
            _spinTween = null;
            if (_onComplete != null)
            {
                _onComplete();
            }
        }

        private void UpdateTicks()
        {
            float eulerZ = _wheelTransform.localEulerAngles.z;
            _unwrappedAngle += Mathf.DeltaAngle(_lastEulerZ, eulerZ);
            _lastEulerZ = eulerZ;

            int slice = SliceUnderPointer(_pointerAngle, _sliceAngle);
            if (slice == _lastSliceUnderPointer)
            {
                return;
            }
            _lastSliceUnderPointer = slice;

            if (!_tickClip || Time.time - _lastTickTime < _minTickInterval)
            {
                return;
            }
            _lastTickTime = Time.time;
            int pitchStep = _random.Range(-UiEffects.TICK_PITCH_STEPS, UiEffects.TICK_PITCH_STEPS + 1);
            _audioSource.pitch = 1f + UiEffects.TICK_PITCH_VARIATION * pitchStep / UiEffects.TICK_PITCH_STEPS;
            _audioSource.PlayOneShot(_tickClip);
        }

        #endregion

        #region Angles

        // Degrees clockwise from the top of the wheel to the pointer.
        private float PointerAngle()
        {
            Vector2 direction = _pointer.localPosition - _wheelTransform.localPosition;
            return Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        }

        private int SliceUnderPointer(float pointerAngle, float sliceAngle)
        {
            return Mathf.FloorToInt((pointerAngle + _unwrappedAngle) / sliceAngle + 0.5f);
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _wheelTransform = transform.FindDeepComponent<RectTransform>(WHEEL_TRANSFORM_NAME);
            _pointer = transform.FindDeepComponent<RectTransform>(POINTER_NAME);
            _audioSource = GetComponent<AudioSource>();

            if (!_wheelTransform || !_pointer)
            {
                Debug.LogWarning($"{name}: wheel or pointer not found.", this);
            }
        }
#endif
    }
}
