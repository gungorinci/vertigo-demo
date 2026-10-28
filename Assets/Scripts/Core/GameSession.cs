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
        private readonly Dictionary<TReward, int> _collected = new Dictionary<TReward, int>();
        private readonly IEventBus _bus;

        public GameSession(IEventBus bus)
        {
            _bus = bus;
        }

        public int Zone { get; private set; } = 1;
        public SessionState State { get; private set; } = SessionState.Idle;

        public IReadOnlyDictionary<TReward, int> Collected
        {
            get { return _collected; }
        }

        public bool IsOver
        {
            get { return State == SessionState.Lost || State == SessionState.Left; }
        }

        public bool CanSpin
        {
            get { return State == SessionState.Idle; }
        }

        public bool CanLeave
        {
            get { return !IsOver && ZoneRules.CanLeave(State == SessionState.Spinning, _collected.Count > 0); }
        }

        #region Public API

        public void BeginSpin()
        {
            if (!CanSpin)
            {
                throw new InvalidOperationException($"Cannot spin while {State}.");
            }
            SetState(SessionState.Spinning);
        }

        public void CompleteSpin(TReward reward, int amount, bool isBomb)
        {
            if (State != SessionState.Spinning)
            {
                throw new InvalidOperationException("No spin in progress.");
            }

            if (isBomb)
            {
                ClearRewards();
                SetState(SessionState.Lost);
                return;
            }

            _collected.TryGetValue(reward, out int total);
            total += amount;
            _collected[reward] = total;
            _bus.Publish(new RewardChangedEvent<TReward>(reward, total));

            Zone++;
            PublishZone();
            SetState(SessionState.Idle);
        }

        public void Leave()
        {
            if (!CanLeave)
            {
                throw new InvalidOperationException($"Cannot leave in zone {Zone} while {State}.");
            }
            SetState(SessionState.Left);
        }

        public void Restart()
        {
            ClearRewards();
            Zone = 1;
            PublishZone();
            SetState(SessionState.Idle);
        }

        public void AnnounceState()
        {
            _bus.Publish(new SessionStateChangedEvent(State, CanSpin, CanLeave));
        }

        #endregion

        #region Internals

        private void ClearRewards()
        {
            _collected.Clear();
            _bus.Publish(new RewardsClearedEvent());
        }

        private void SetState(SessionState state)
        {
            State = state;
            AnnounceState();
        }

        private void PublishZone()
        {
            _bus.Publish(new ZoneChangedEvent(Zone));
        }

        #endregion
    }
}
