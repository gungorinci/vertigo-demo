namespace VertigoDemo.Core
{
    public readonly struct ZoneChangedEvent
    {
        public readonly int Zone;
        public readonly ZoneType ZoneType;

        public ZoneChangedEvent(int zone, ZoneType zoneType)
        {
            Zone = zone;
            ZoneType = zoneType;
        }
    }

    public readonly struct SessionStateChangedEvent
    {
        public readonly SessionState State;

        public SessionStateChangedEvent(SessionState state)
        {
            State = state;
        }
    }

    public readonly struct RewardChangedEvent<TReward>
    {
        public readonly TReward Reward;
        public readonly int Total;

        public RewardChangedEvent(TReward reward, int total)
        {
            Reward = reward;
            Total = total;
        }
    }

    public readonly struct RewardsClearedEvent
    {
    }

    public readonly struct InventoryChangedEvent
    {
    }

    public readonly struct RestartRequestedEvent
    {
    }
}
