using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.Data;
using VertigoDemo.Game;

namespace VertigoDemo.UI
{
    public class WheelView : MonoBehaviour
    {
        private const string WheelImageName = "ui_image_spin_wheel_value";
        private const string PointerImageName = "ui_image_pointer_value";

        [SerializeField, HideInInspector] private Image wheelImage;
        [SerializeField, HideInInspector] private Image pointerImage;
        [SerializeField, HideInInspector] private WheelSliceView[] sliceViews;

        public void Show(WheelConfig config, IReadOnlyList<RolledSlice> slices)
        {
            wheelImage.sprite = config.WheelSprite;
            pointerImage.sprite = config.IndicatorSprite;

            for (int i = 0; i < sliceViews.Length && i < slices.Count; i++)
            {
                RolledSlice slice = slices[i];
                if (slice.Reward == null) continue;
                sliceViews[i].Show(slice.Reward.Icon, slice.Amount);
            }
        }

        public RectTransform GetSliceIcon(int index) => sliceViews[index].IconTransform;

#if UNITY_EDITOR
        private void OnValidate()
        {
            wheelImage = FindImage(WheelImageName);
            pointerImage = FindImage(PointerImageName);
            sliceViews = GetComponentsInChildren<WheelSliceView>(true);
        }

        private Image FindImage(string childName)
        {
            Transform child = transform.FindDeep(childName);
            if (child == null)
            {
                Debug.LogWarning($"{name}: child '{childName}' not found.", this);
                return null;
            }
            return child.GetComponent<Image>();
        }
#endif
    }
}