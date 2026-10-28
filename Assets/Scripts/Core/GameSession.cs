using System;
using System.Collections.Generic;

namespace VertigoDemo.Core
{
    public enum SessionState
    {
        Idle,
        Spinning,
        Lost,
        Left
    }

    public class GameSession<TReward>
    {
        private readonly Dictionary<TReward, int> collected = new Dictionary<TReward, int>();
        private readonly IEventBus _bus;

        public GameSession(IEventBus bus)
        {
            _bus = bus;
        }

        public int Zone { get; private set; } = 1;
        public SessionState State { get; private set; } = SessionState.Idle;
        public IReadOnlyDictionary<TReward, int> Collected => collected;

        public bool IsOver => State == SessionState.Lost || State == SessionState.Left;
        public bool CanSpin => State == SessionState.Idle;
        public bool CanLeave => !IsOver && ZoneRules.CanLeave(State == SessionState.Spinning, collected.Count > 0);

        public void BeginSpin()
        {
            if (!CanSpin) throw new InvalidOperationException($"Cannot spin while {State}.");
            SetState(SessionState.Spinning);
        }

        public void CompleteSpin(TReward reward, int amount, bool isBomb)
        {
            if (State != SessionState.Spinning) throw new InvalidOperationException("No spin in progress.");

            if (isBomb)
            {
                ClearRewards();
                SetState(SessionState.Lost);
                return;
            }

            collected.TryGetValue(reward, out int total);
            total += amount;
            collected[reward] = total;
            _bus.Publish(new RewardChangedEvent<TReward>(reward, total));

            Zone++;
            PublishZone();
            SetState(SessionState.Idle);
        }

        public void Leave()
        {
            if (!CanLeave) throw new InvalidOperationException($"Cannot leave in zone {Zone} while {State}.");
            SetState(SessionState.Left);
        }

        public void Restart()
        {
            ClearRewards();
            Zone = 1;
            PublishZone();
            SetState(SessionState.Idle);
        }

        private void ClearRewards()
        {
            collected.Clear();
            _bus.Publish(new RewardsClearedEvent());
        }

        private void SetState(SessionState state)
        {
            State = state;
            AnnounceState();
        }

        public void AnnounceState()
        {
            _bus.Publish(new SessionStateChangedEvent(State, CanSpin, CanLeave));
        }

        private void PublishZone()
        {
            _bus.Publish(new ZoneChangedEvent(Zone));
        }
    }
}