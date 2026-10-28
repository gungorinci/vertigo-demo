using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private List<ZoneTypeData> _zoneTypes = new List<ZoneTypeData>();
        [SerializeField] private RandomSettings _randomSettings;

        [SerializeField, Min(0f)] private float resultDelay = 1f;
        [SerializeField, Min(0f)] private float flyDelay = 0.4f;

        [SerializeField, HideInInspector] private ControlsView controls;
        [SerializeField, HideInInspector] private WheelView wheelView;
        [SerializeField, HideInInspector] private WheelSpinAnimator spinAnimator;
        [SerializeField, HideInInspector] private ResultPanelView resultPanel;
        [SerializeField, HideInInspector] private HudView hud;
        [SerializeField, HideInInspector] private RewardHudView rewardHud;
        [SerializeField, HideInInspector] private WheelWinEffect winEffect;
        [SerializeField, HideInInspector] private RewardFlyEffect rewardFly;
        [SerializeField, HideInInspector] private WalletView walletView;

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
            if (!session.CanLeave) return;

            session.Leave();
        }

        private void OnRestartRequested(RestartRequestedEvent evt)
        {
            session.Restart();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            controls = GetComponentInChildren<ControlsView>(true);
            wheelView = GetComponentInChildren<WheelView>(true);
            spinAnimator = GetComponentInChildren<WheelSpinAnimator>(true);

            resultPanel = GetComponentInChildren<ResultPanelView>(true);
            hud = GetComponentInChildren<HudView>(true);
            rewardHud = GetComponentInChildren<RewardHudView>(true);
            winEffect = GetComponentInChildren<WheelWinEffect>(true);
            rewardFly = GetComponentInChildren<RewardFlyEffect>(true);
            walletView = GetComponentInChildren<WalletView>(true);

            if (!controls || !wheelView || !spinAnimator
                || !resultPanel || !hud || !rewardHud || !winEffect || !rewardFly
                || !walletView)
                Debug.LogWarning($"{name}: a button or view was not found.", this);
        }
#endif
    }
}