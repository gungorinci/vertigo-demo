using System.Collections.Generic;
using UnityEngine;

namespace VertigoDemo.Data
{
    [CreateAssetMenu(fileName = "Wheel_", menuName = "Vertigo Demo/Wheel Config")]
    public class WheelConfig : ScriptableObject
    {
        [SerializeField] private Sprite wheelSprite;
        [SerializeField] private Sprite indicatorSprite;
        [SerializeField] private List<WheelSlice> slices = new List<WheelSlice>();
        [SerializeField, Range(0f, 0.5f)] private float _growthPerZone = 0.08f;

        public Sprite WheelSprite => wheelSprite;
        public Sprite IndicatorSprite => indicatorSprite;
        public IReadOnlyList<WheelSlice> Slices => slices;

        public float GrowthPerZone
        {
            get { return _growthPerZone; }
        }
    }
}