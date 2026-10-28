using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VertigoDemo.Data
{
    [CreateAssetMenu(fileName = "Wheel_", menuName = "Vertigo Demo/Wheel Config")]
    public class WheelConfig : ScriptableObject
    {
        private const int SLOT_COUNT = 8;

        [SerializeField, FormerlySerializedAs("wheelSprite")] private Sprite _wheelSprite;
        [SerializeField, FormerlySerializedAs("indicatorSprite")] private Sprite _indicatorSprite;
        [SerializeField, FormerlySerializedAs("slices")] private List<WheelSlice> _slices = new List<WheelSlice>();
        [SerializeField, Range(0f, 0.5f)] private float _growthPerZone = 0.08f;

        public Sprite WheelSprite
        {
            get { return _wheelSprite; }
        }

        public Sprite IndicatorSprite
        {
            get { return _indicatorSprite; }
        }

        public IReadOnlyList<WheelSlice> Slices
        {
            get { return _slices; }
        }

        public float GrowthPerZone
        {
            get { return _growthPerZone; }
        }

        #region Public API

        public int CountBombSlices()
        {
            int bombSlices = 0;
            foreach (WheelSlice slice in _slices)
            {
                foreach (RewardData reward in slice.RewardPool)
                {
                    if (reward && reward.IsBomb)
                    {
                        bombSlices++;
                        break;
                    }
                }
            }
            return bombSlices;
        }

        #endregion

        #region Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_slices.Count != SLOT_COUNT)
            {
                LogProblem($"has {_slices.Count} slices, the wheel has {SLOT_COUNT} slots.");
            }

            for (int i = 0; i < _slices.Count; i++)
            {
                WheelSlice slice = _slices[i];
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
                if (bombs > 0 && bombs != rewards)
                {
                    LogProblem($"slice {i} mixes the bomb with other rewards.");
                }
                if (slice.MaxAmount < slice.MinAmount)
                {
                    LogProblem($"slice {i} has Max Amount below Min Amount.");
                }
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
