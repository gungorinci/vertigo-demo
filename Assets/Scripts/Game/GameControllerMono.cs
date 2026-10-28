using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class GameControllerMono : MonoBehaviour
    {
        [SerializeField] private List<ZoneTypeData> _zoneTypes = new List<ZoneTypeData>();
        [SerializeField] private RandomSettings _randomSettings;

        [SerializeField, Min(0f)] private float resultDelay = 1f;
        [SerializeField, Min(0f)] private float flyDelay = 0.4f;

        [SerializeField, HideInInspector] private ControlsViewMono controls;
        [SerializeField, HideInInspector] private WheelViewMono wheelView;
        [SerializeField, HideInInspector] private WheelSpinAnimatorMono spinAnimator;
        [SerializeField, HideInInspector] private ResultPanelViewMono resultPanel;
        [SerializeField, HideInInspector] private HudViewMono hud;
        [SerializeField, HideInInspector] private RewardHudViewMono rewardHud;
        [SerializeField, HideInInspector] private WheelWinEffectMono winEffect;
        [SerializeField, HideInInspector] private RewardFlyEffectMono rewardFly;
        [SerializeField, HideInInspector] private WalletViewMono walletView;

        private IRandomSource random;
        private readonly IEventBus bus = new EventBus();
        private GameSession<RewardData> session;
        private SpinFlow spinFlow;
        private EconomyPresenter economy;
        private ZonePresenter zones;

        private void Start()
        {
            List<ZoneTypeData> zoneTypes = CollectZoneTypes();
            random = _randomSettings.CreateSource();
            session = new GameSession<RewardData>(bus);
            resultPanel.Bind(bus);
            controls.Bind(bus);
            rewardHud.Bind(bus);
            spinFlow = new SpinFlow(bus, session, random, wheelView, spinAnimator, winEffect,
                rewardHud, rewardFly, gameObject, resultDelay, flyDelay);
            economy = new EconomyPresenter(bus, session, new FileSaveStore(), walletView, CollectRewards(zoneTypes));
            zones = new ZonePresenter(bus, session, new ZoneSchedule<ZoneTypeData>(zoneTypes),
                random, spinFlow, wheelView, winEffect, hud);
            Subscribe();
            zones.ShowZone();
            session.AnnounceState();
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
            bus.Subscribe<RestartRequestedEvent>(OnRestartRequested);
            bus.Subscribe<LeaveRequestedEvent>(OnLeaveRequested);
        }

        private void OnDestroy()
        {
            bus.Unsubscribe<RestartRequestedEvent>(OnRestartRequested);
            bus.Unsubscribe<LeaveRequestedEvent>(OnLeaveRequested);
            if (spinFlow != null)
            {
                spinFlow.Dispose();
            }
            if (economy != null)
            {
                economy.Dispose();
            }
            if (zones != null)
            {
                zones.Dispose();
            }
        }

        private void OnLeaveRequested(LeaveRequestedEvent evt)
        {
            if (!session.CanLeave)
            {
                Debug.LogWarning($"Leave request ignored in zone {session.Zone} while the session is {session.State}.");
                return;
            }

            session.Leave();
        }

        private void OnRestartRequested(RestartRequestedEvent evt)
        {
            session.Restart();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            controls = GetComponentInChildren<ControlsViewMono>(true);
            wheelView = GetComponentInChildren<WheelViewMono>(true);
            spinAnimator = GetComponentInChildren<WheelSpinAnimatorMono>(true);

            resultPanel = GetComponentInChildren<ResultPanelViewMono>(true);
            hud = GetComponentInChildren<HudViewMono>(true);
            rewardHud = GetComponentInChildren<RewardHudViewMono>(true);
            winEffect = GetComponentInChildren<WheelWinEffectMono>(true);
            rewardFly = GetComponentInChildren<RewardFlyEffectMono>(true);
            walletView = GetComponentInChildren<WalletViewMono>(true);

            if (!controls || !wheelView || !spinAnimator
                || !resultPanel || !hud || !rewardHud || !winEffect || !rewardFly
                || !walletView)
                Debug.LogWarning($"{name}: a button or view was not found.", this);
        }
#endif
    }
}