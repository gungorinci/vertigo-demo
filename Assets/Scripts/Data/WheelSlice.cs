using System;
using System.Collections.Generic;
using UnityEngine;

namespace VertigoDemo.Data
{
    [Serializable]
    public class WheelSlice
    {
        [SerializeField] private List<RewardData> rewardPool = new List<RewardData>();
        [SerializeField, Min(1)] private int minAmount = 1;
        [SerializeField, Min(1)] private int maxAmount = 1;
        [SerializeField, Min(0)] private int weight = 10;

        public IReadOnlyList<RewardData> RewardPool => rewardPool;
        public int MinAmount => minAmount;
        public int MaxAmount => maxAmount;
        public int Weight => weight;
    }
}