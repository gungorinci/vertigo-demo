using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class SpinFlow : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly GameSession<RewardData> _session;
        private readonly IRandomSource _random;
        private readonly WheelViewMono _wheelView;
        private readonly WheelSpinAnimatorMono _spinAnimator;
        private readonly WheelWinEffectMono _winEffect;
        private readonly RewardHudViewMono _rewardHud;
        private readonly RewardFlyEffectMono _rewardFly;
        private readonly GameObject _owner;
        private readonly float _resultDelay;
        private readonly float _flyDelay;

        private IReadOnlyList<RolledSlice> _slices;
        private string _wheelName;
        private int _winnerIndex;
        private RolledSlice _winner;

        public SpinFlow(
            IEventBus bus,
            GameSession<RewardData> session,
            IRandomSource random,
            WheelViewMono wheelView,
            WheelSpinAnimatorMono spinAnimator,
            WheelWinEffectMono winEffect,
            RewardHudViewMono rewardHud,
            RewardFlyEffectMono rewardFly,
            GameObject owner,
            float resultDelay,
            float flyDelay)
        {
            _bus = bus;
            _session = session;
            _random = random;
            _wheelView = wheelView;
            _spinAnimator = spinAnimator;
            _winEffect = winEffect;
            _rewardHud = rewardHud;
            _rewardFly = rewardFly;
            _owner = owner;
            _resultDelay = resultDelay;
            _flyDelay = flyDelay;

            _bus.Subscribe<SpinRequestedEvent>(OnSpinRequested);
        }

        #region Public API

        public void SetSlices(IReadOnlyList<RolledSlice> slices, string wheelName)
        {
            _slices = slices;
            _wheelName = wheelName;
        }

        public void Dispose()
        {
            _bus.Unsubscribe<SpinRequestedEvent>(OnSpinRequested);
        }

        #endregion

        #region Flow

        private void OnSpinRequested(SpinRequestedEvent evt)
        {
            if (!_session.CanSpin)
            {
                Debug.LogWarning($"Spin request ignored while the session is {_session.State}.");
                return;
            }

            var weights = new List<int>(_slices.Count);
            int totalWeight = 0;
            foreach (RolledSlice slice in _slices)
            {
                weights.Add(slice.Weight);
                totalWeight += slice.Weight;
            }

            if (totalWeight == 0)
            {
                Debug.LogError($"No slice can be picked on '{_wheelName}'.");
                return;
            }

            _winnerIndex = WeightedPicker.Pick(weights, _random);
            _winner = _slices[_winnerIndex];

            _session.BeginSpin();
            _bus.Publish(new SpinStartedEvent());
            _spinAnimator.Spin(_winnerIndex, _slices.Count, _random, OnWheelStopped);
        }

        private void OnWheelStopped()
        {
            _winEffect.Play(_winnerIndex);

            if (_winner.Reward.IsBomb)
            {
                DOVirtual.DelayedCall(_resultDelay, CompleteSpin).SetLink(_owner);
                return;
            }

            _rewardHud.Reveal(_winner.Reward);
            DOVirtual.DelayedCall(_flyDelay, FlyReward).SetLink(_owner);
        }

        private void FlyReward()
        {
            RectTransform from = _wheelView.GetSliceIcon(_winnerIndex);
            Vector3 to = _rewardHud.GetTargetPosition(_winner.Reward);
            _rewardFly.Fly(_winner.Reward.Icon, from, to, CompleteSpin);
        }

        private void CompleteSpin()
        {
            _session.CompleteSpin(_winner.Reward, _winner.Amount, _winner.Reward.IsBomb);
        }

        #endregion
    }
}
