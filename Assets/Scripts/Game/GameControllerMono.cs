using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class GameControllerMono : MonoBehaviour
    {
        [SerializeField] private List<ZoneTypeData> _zoneTypes = new List<ZoneTypeData>();
        [SerializeField] private RandomSettings _randomSettings;

        [SerializeField, Min(0f), FormerlySerializedAs("resultDelay")] private float _resultDelay = 1f;
        [SerializeField, Min(0f), FormerlySerializedAs("flyDelay")] private float _flyDelay = 0.4f;

        [SerializeField, HideInInspector, FormerlySerializedAs("controls")] private ControlsViewMono _controls;
        [SerializeField, HideInInspector, FormerlySerializedAs("wheelView")] private WheelViewMono _wheelView;
        [SerializeField, HideInInspector, FormerlySerializedAs("spinAnimator")] private WheelSpinAnimatorMono _spinAnimator;
        [SerializeField, HideInInspector, FormerlySerializedAs("resultPanel")] private ResultPanelViewMono _resultPanel;
        [SerializeField, HideInInspector, FormerlySerializedAs("hud")] private HudViewMono _hud;
        [SerializeField, HideInInspector, FormerlySerializedAs("rewardHud")] private RewardHudViewMono _rewardHud;
        [SerializeField, HideInInspector, FormerlySerializedAs("winEffect")] private WheelWinEffectMono _winEffect;
        [SerializeField, HideInInspector, FormerlySerializedAs("rewardFly")] private RewardFlyEffectMono _rewardFly;
        [SerializeField, HideInInspector, FormerlySerializedAs("walletView")] private WalletViewMono _walletView;

        private IRandomSource _random;
        private readonly IEventBus _bus = new EventBus();
        private GameSession<RewardData> _session;
        private SpinFlow _spinFlow;
        private EconomyPresenter _economy;
        private ZonePresenter _zones;

        private void Start()
        {
            List<ZoneTypeData> zoneTypes = CollectZoneTypes();
            _random = _randomSettings.CreateSource();
            _session = new GameSession<RewardData>(_bus);
            _resultPanel.Bind(_bus);
            _controls.Bind(_bus);
            _rewardHud.Bind(_bus);
            _spinFlow = new SpinFlow(_bus, _session, _random, _wheelView, _spinAnimator, _winEffect,
                _rewardHud, _rewardFly, gameObject, _resultDelay, _flyDelay);
            _economy = new EconomyPresenter(_bus, _session, new FileSaveStore(), _walletView, CollectRewards(zoneTypes));
            _zones = new ZonePresenter(_bus, _session, new ZoneSchedule<ZoneTypeData>(zoneTypes),
                _random, _spinFlow, _wheelView, _winEffect, _hud);
            Subscribe();
            _zones.ShowZone();
            _session.AnnounceState();
        }

        // ZoneSchedule lives in Core and cannot see Unity's fake null, so empty or missing slots are dropped here.
        private List<ZoneTypeData> CollectZoneTypes()
        {
            var zoneTypes = new List<ZoneTypeData>();
            foreach (ZoneTypeData zoneType in _zoneTypes)
            {
                if (zoneType)
                {
                    zoneTypes.Add(zoneType);
                }
            }
            return zoneTypes;
        }

        private List<RewardData> CollectRewards(List<ZoneTypeData> zoneTypes)
        {
            var rewards = new List<RewardData>();
            foreach (ZoneTypeData zoneType in zoneTypes)
            {
                foreach (WheelSlice slice in zoneType.Wheel.Slices)
                {
                    foreach (RewardData reward in slice.RewardPool)
                    {
                        if (reward && !rewards.Contains(reward))
                        {
                            rewards.Add(reward);
                        }
                    }
                }
            }
            return rewards;
        }

        private void Subscribe()
        {
            _bus.Subscribe<RestartRequestedEvent>(OnRestartRequested);
            _bus.Subscribe<LeaveRequestedEvent>(OnLeaveRequested);
        }

        private void OnDestroy()
        {
            _bus.Unsubscribe<RestartRequestedEvent>(OnRestartRequested);
            _bus.Unsubscribe<LeaveRequestedEvent>(OnLeaveRequested);
            if (_spinFlow != null)
            {
                _spinFlow.Dispose();
            }
            if (_economy != null)
            {
                _economy.Dispose();
            }
            if (_zones != null)
            {
                _zones.Dispose();
            }
        }

        private void OnLeaveRequested(LeaveRequestedEvent evt)
        {
            if (!_session.CanLeave)
            {
                Debug.LogWarning($"Leave request ignored in zone {_session.Zone} while the session is {_session.State}.");
                return;
            }

            _session.Leave();
        }

        private void OnRestartRequested(RestartRequestedEvent evt)
        {
            _session.Restart();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _controls = GetComponentInChildren<ControlsViewMono>(true);
            _wheelView = GetComponentInChildren<WheelViewMono>(true);
            _spinAnimator = GetComponentInChildren<WheelSpinAnimatorMono>(true);

            _resultPanel = GetComponentInChildren<ResultPanelViewMono>(true);
            _hud = GetComponentInChildren<HudViewMono>(true);
            _rewardHud = GetComponentInChildren<RewardHudViewMono>(true);
            _winEffect = GetComponentInChildren<WheelWinEffectMono>(true);
            _rewardFly = GetComponentInChildren<RewardFlyEffectMono>(true);
            _walletView = GetComponentInChildren<WalletViewMono>(true);

            if (!_controls || !_wheelView || !_spinAnimator
                || !_resultPanel || !_hud || !_rewardHud || !_winEffect || !_rewardFly
                || !_walletView)
                Debug.LogWarning($"{name}: a button or view was not found.", this);
        }
#endif
    }
}