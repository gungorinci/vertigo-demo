using System;

namespace VertigoDemo.Core
{
    public static class RewardScaling
    {
        #region Public API

        public static float Multiplier(int zone, float growthPerZone)
        {
            return 1f + growthPerZone * (zone - 1);
        }

        public static int ScaleAmount(int baseAmount, int zone, float growthPerZone)
        {
            double scaled = baseAmount * Multiplier(zone, growthPerZone);
            return Math.Max(1, (int)Math.Round(scaled, MidpointRounding.AwayFromZero));
        }

        #endregion
    }
}
