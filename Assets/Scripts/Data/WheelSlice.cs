using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VertigoDemo.Data
{
    [Serializable]
    public class WheelSlice
    {
        [SerializeField, FormerlySerializedAs("rewardPool")] private List<RewardData> _rewardPool = new List<RewardData>();
        [SerializeField, Min(1), FormerlySerializedAs("minAmount")] private int _minAmount = 1;
        [SerializeField, Min(1), FormerlySerializedAs("maxAmount")] private int _maxAmount = 1;
        [SerializeField, Min(0), FormerlySerializedAs("weight")] private int _weight = 10;

        public IReadOnlyList<RewardData> RewardPool => _rewardPool;
        public int MinAmount => _minAmount;
        public int MaxAmount => _maxAmount;
        public int Weight => _weight;
    }
}