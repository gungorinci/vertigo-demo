using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.Data;

namespace VertigoDemo.UI
{
    public class WheelView : MonoBehaviour
    {
        private const string WheelImageName = "ui_image_spin_wheel";
        private const string PointerImageName = "ui_image_pointer";

        [SerializeField, HideInInspector] private Image wheelImage;
        [SerializeField, HideInInspector] private Image pointerImage;
        [SerializeField, HideInInspector] private WheelSliceView[] sliceViews;

        // Temporary: lets us see a config in Play mode before GameController exists.
        [SerializeField] private WheelConfig previewConfig;

        public void Show(WheelConfig config)
        {
            wheelImage.sprite = config.WheelSprite;
            pointerImage.sprite = config.IndicatorSprite;

            // Temporary: first reward and min amount, until rewards are rolled per zone.
            for (int i = 0; i < sliceViews.Length && i < config.Slices.Count; i++)
            {
                WheelSlice slice = config.Slices[i];
                if (slice.RewardPool.Count == 0) continue;
                sliceViews[i].Show(slice.RewardPool[0].Icon, slice.MinAmount);
            }
        }

        private void Start()
        {
            if (previewConfig != null) Show(previewConfig);
        }

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