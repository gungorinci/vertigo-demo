namespace VertigoDemo.Core
{
    public readonly struct ZoneChangedEvent
    {
        public readonly int Zone;

        public ZoneChangedEvent(int zone)
        {
            Zone = zone;
        }
    }

    public readonly struct SessionStateChangedEvent
    {
        public readonly SessionState State;
        public readonly bool CanSpin;
        public readonly bool CanLeave;

        public SessionStateChangedEvent(SessionState state, bool canSpin, bool canLeave)
        {
            State = state;
            CanSpin = canSpin;
            CanLeave = canLeave;
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

    public readonly struct SpinRequestedEvent
    {
    }

    public readonly struct SpinStartedEvent
    {
    }

    public readonly struct LeaveRequestedEvent
    {
    }
}
