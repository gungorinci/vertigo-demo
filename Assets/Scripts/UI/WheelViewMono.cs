using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VertigoDemo.Data;

namespace VertigoDemo.UI
{
    public class WheelViewMono : MonoBehaviour
    {
        private const string WHEEL_IMAGE_NAME = "ui_image_spin_wheel_value";
        private const string POINTER_IMAGE_NAME = "ui_image_pointer_value";

        [SerializeField, HideInInspector, FormerlySerializedAs("wheelImage")] private Image _wheelImage;
        [SerializeField, HideInInspector, FormerlySerializedAs("pointerImage")] private Image _pointerImage;
        [SerializeField, HideInInspector, FormerlySerializedAs("sliceViews")] private WheelSliceViewMono[] _sliceViews;

        public void Show(WheelConfig config, IReadOnlyList<RolledSlice> slices)
        {
            _wheelImage.sprite = config.WheelSprite;
            _pointerImage.sprite = config.IndicatorSprite;

            for (int i = 0; i < _sliceViews.Length && i < slices.Count; i++)
            {
                RolledSlice slice = slices[i];
                if (!slice.Reward) continue;
                _sliceViews[i].Show(slice.Reward.Icon, slice.Amount);
            }
        }

        public RectTransform GetSliceIcon(int index) => _sliceViews[index].IconTransform;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _wheelImage = FindImage(WHEEL_IMAGE_NAME);
            _pointerImage = FindImage(POINTER_IMAGE_NAME);
            _sliceViews = GetComponentsInChildren<WheelSliceViewMono>(true);
        }

        private Image FindImage(string childName)
        {
            Transform child = transform.FindDeep(childName);
            if (!child)
            {
                Debug.LogWarning($"{name}: child '{childName}' not found.", this);
                return null;
            }
            return child.GetComponent<Image>();
        }
#endif
    }
}