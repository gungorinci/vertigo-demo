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

        public int Zone { get; private set; } = 1;
        public SessionState State { get; private set; } = SessionState.Idle;
        public ZoneType ZoneType => ZoneRules.GetZoneType(Zone);
        public IReadOnlyDictionary<TReward, int> Collected => collected;

        public bool IsOver => State == SessionState.Lost || State == SessionState.Left;
        public bool CanSpin => State == SessionState.Idle;
        public bool CanLeave => !IsOver && ZoneRules.CanLeave(ZoneType, State == SessionState.Spinning);

        public event Action<int> ZoneChanged;              // new zone
        public event Action<TReward, int> RewardChanged;   // reward, new total
        public event Action RewardsCleared;
        public event Action<SessionState> StateChanged;

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
            RewardChanged?.Invoke(reward, total);

            Zone++;
            ZoneChanged?.Invoke(Zone);
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
            ZoneChanged?.Invoke(Zone);
            SetState(SessionState.Idle);
        }

        private void ClearRewards()
        {
            collected.Clear();
            RewardsCleared?.Invoke();
        }

        private void SetState(SessionState state)
        {
            State = state;
            StateChanged?.Invoke(state);
        }
    }
}