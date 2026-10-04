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

        public Sprite WheelSprite => wheelSprite;
        public Sprite IndicatorSprite => indicatorSprite;
        public IReadOnlyList<WheelSlice> Slices => slices;
    }
}