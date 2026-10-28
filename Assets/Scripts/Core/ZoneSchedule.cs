using System;
using System.Collections.Generic;

namespace VertigoDemo.Core
{
    public class ZoneSchedule<TZoneType> where TZoneType : class, IZoneType
    {
        private const int FIRST_ZONE = 1;
        private const int EVERY_ZONE = 1;

        private readonly List<TZoneType> _types;
        private readonly bool _hasSpecialTypes;

        public ZoneSchedule(IEnumerable<TZoneType> types)
        {
            _types = new List<TZoneType>();
            bool hasEveryZoneType = false;
            foreach (TZoneType type in types)
            {
                if (type == null)
                {
                    continue;
                }
                if (type.Interval < EVERY_ZONE)
                {
                    throw new ArgumentException($"A zone type has interval {type.Interval}, it must be at least {EVERY_ZONE}.");
                }
                if (type.Interval == EVERY_ZONE)
                {
                    hasEveryZoneType = true;
                }
                else
                {
                    _hasSpecialTypes = true;
                }
                _types.Add(type);
            }

            if (!hasEveryZoneType)
            {
                throw new ArgumentException($"One zone type needs interval {EVERY_ZONE}, so every zone has a type.");
            }
        }

        public IReadOnlyList<TZoneType> Types
        {
            get { return _types; }
        }

        #region Public API

        // The type with the largest interval that divides the zone wins: zone 30 is Super, not Safe.
        public TZoneType For(int zone)
        {
            if (zone < FIRST_ZONE)
            {
                throw new ArgumentOutOfRangeException(nameof(zone), zone, $"Zones start at {FIRST_ZONE}.");
            }

            TZoneType best = null;
            foreach (TZoneType type in _types)
            {
                if (zone % type.Interval != 0)
                {
                    continue;
                }
                if (best == null || type.Interval > best.Interval)
                {
                    best = type;
                }
            }
            return best;
        }

        public bool TryFindNextSpecialZone(int zone, out int nextZone)
        {
            nextZone = zone + 1;
            if (!_hasSpecialTypes)
            {
                return false;
            }

            while (For(nextZone).Interval == EVERY_ZONE)
            {
                nextZone++;
            }
            return true;
        }

        #endregion
    }
}
