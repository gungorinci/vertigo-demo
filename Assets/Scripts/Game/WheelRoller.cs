using System.Collections.Generic;
using VertigoDemo.Core;
using VertigoDemo.Data;

namespace VertigoDemo.Game
{
    public static class WheelRoller
    {
        public static List<RolledSlice> Roll(WheelConfig config, IRandomSource random)
        {
            var rolled = new List<RolledSlice>(config.Slices.Count);

            foreach (WheelSlice slice in config.Slices)
            {
                RewardData reward = slice.RewardPool.Count > 0
                    ? slice.RewardPool[random.Range(0, slice.RewardPool.Count)]
                    : null;

                int amount;
                if (reward != null && reward.IsBomb)
                {
                    amount = 1; // a bomb has no amount, always shown as x1
                }
                else
                {
                    int max = slice.MaxAmount < slice.MinAmount ? slice.MinAmount : slice.MaxAmount;
                    amount = random.Range(slice.MinAmount, max + 1);
                }
                rolled.Add(new RolledSlice(reward, amount, slice.Weight));
            }

            return rolled;
        }
    }
}