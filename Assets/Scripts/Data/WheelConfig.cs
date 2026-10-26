using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.Core;

namespace VertigoDemo.Data
{
    [CreateAssetMenu(fileName = "Wheel_", menuName = "Vertigo Demo/Wheel Config")]
    public class WheelConfig : ScriptableObject
    {
        private const int SLOT_COUNT = 8;

        [SerializeField] private ZoneType _zoneType;
        [SerializeField] private Sprite wheelSprite;
        [SerializeField] private Sprite indicatorSprite;
        [SerializeField] private List<WheelSlice> slices = new List<WheelSlice>();
        [SerializeField, Range(0f, 0.5f)] private float _growthPerZone = 0.08f;

        public Sprite WheelSprite => wheelSprite;
        public Sprite IndicatorSprite => indicatorSprite;
        public IReadOnlyList<WheelSlice> Slices => slices;

        public ZoneType ZoneType
        {
            get { return _zoneType; }
        }

        public float GrowthPerZone
        {
            get { return _growthPerZone; }
        }

        #region Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (slices.Count != SLOT_COUNT)
            {
                LogProblem($"has {slices.Count} slices, the wheel has {SLOT_COUNT} slots.");
            }

            int bombSlices = 0;
            for (int i = 0; i < slices.Count; i++)
            {
                WheelSlice slice = slices[i];
                int rewards = 0;
                int bombs = 0;

                foreach (RewardData reward in slice.RewardPool)
                {
                    if (!reward)
                    {
                        continue;
                    }
                    rewards++;
                    if (reward.IsBomb)
                    {
                        bombs++;
                    }
                }

                if (rewards == 0)
                {
                    LogProblem($"slice {i} has an empty reward pool.");
                }
                if (bombs > 0)
                {
                    bombSlices++;
                    if (bombs != rewards)
                    {
                        LogProblem($"slice {i} mixes the bomb with other rewards.");
                    }
                }
                if (slice.MaxAmount < slice.MinAmount)
                {
                    LogProblem($"slice {i} has Max Amount below Min Amount.");
                }
            }

            int expectedBombSlices = ZoneRules.HasBomb(_zoneType) ? 1 : 0;
            if (bombSlices != expectedBombSlices)
            {
                LogProblem($"a {_zoneType} wheel needs {expectedBombSlices} bomb slice(s), this one has {bombSlices}.");
            }
        }

        private void LogProblem(string problem)
        {
            Debug.LogError($"{name}: {problem}", this);
        }
#endif

        #endregion
    }
}
