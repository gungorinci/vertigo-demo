
using System;

namespace VertigoDemo.Core
{
    public enum ZoneType
    {
        Normal,
        Safe,
        Super
    }


    public static class ZoneRules
    {
        public const int SafeInterval = 5;
        public const int SuperInterval = 30;

        public static ZoneType GetZoneType(int zoneValue)
        {
            if (zoneValue < 1)
                throw new ArgumentOutOfRangeException(nameof(zoneValue), zoneValue, "Zones start at 1.");

            if (zoneValue % SuperInterval == 0) return ZoneType.Super;
            if (zoneValue % SafeInterval == 0) return ZoneType.Safe;
            return ZoneType.Normal;
        }

        public static bool HasBomb(ZoneType type) => type == ZoneType.Normal;

        public static bool CanLeave(ZoneType type, bool isSpinning) =>
            !isSpinning && type != ZoneType.Normal;
    }
}