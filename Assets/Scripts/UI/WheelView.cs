using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.Game;

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

       private void Start()
       {
            if (previewConfig == null) return;
       
            IRandomSource random = new SystemRandomSource();
            List<RolledSlice> rolled = WheelRoller.Roll(previewConfig, random);
            Show(previewConfig, rolled);
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