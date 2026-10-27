using System.Collections.Generic;
using UnityEngine;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private WheelConfig normalWheel;   // bronze, with the bomb
        [SerializeField] private WheelConfig safeWheel;     // silver, every 5th zone
        [SerializeField] private WheelConfig superWheel;    // gold, every 30th zone

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

        private readonly IRandomSource random = new SystemRandomSource();
        private readonly IEventBus bus = new EventBus();
        private List<RolledSlice> currentSlices;
        private GameSession<RewardData> session;
        private WheelConfig currentConfig;
        private WheelSelector<WheelConfig> wheels;
        private bool showIntro = true;
        private SpinFlow spinFlow;
        private EconomyPresenter economy;

        private void Start()
        {
            wheels = new WheelSelector<WheelConfig>(normalWheel, safeWheel, superWheel);
            session = new GameSession<RewardData>(bus);
            resultPanel.Bind(bus);
            controls.Bind(bus);
            rewardHud.Bind(bus);
            spinFlow = new SpinFlow(bus, session, random, wheelView, spinAnimator, winEffect,
                rewardHud, rewardFly, gameObject, resultDelay, flyDelay);
            economy = new EconomyPresenter(bus, session, new FileSaveStore(), walletView, CollectRewards());
            Subscribe();
            ShowZone();
            session.AnnounceState();
        }

        private List<RewardData> CollectRewards()
        {
            var rewards = new List<RewardData>();
            foreach (WheelConfig wheel in new[] { normalWheel, safeWheel, superWheel })
            {
                foreach (WheelSlice slice in wheel.Slices)
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
            bus.Subscribe<ZoneChangedEvent>(OnZoneChanged);
            bus.Subscribe<RestartRequestedEvent>(OnRestartRequested);
            bus.Subscribe<LeaveRequestedEvent>(OnLeaveRequested);
            bus.Subscribe<SpinStartedEvent>(OnSpinStarted);
        }

        private void OnDestroy()
        {
            bus.Unsubscribe<ZoneChangedEvent>(OnZoneChanged);
            bus.Unsubscribe<RestartRequestedEvent>(OnRestartRequested);
            bus.Unsubscribe<LeaveRequestedEvent>(OnLeaveRequested);
            bus.Unsubscribe<SpinStartedEvent>(OnSpinStarted);
            if (spinFlow != null)
            {
                spinFlow.Dispose();
            }
            if (economy != null)
            {
                economy.Dispose();
            }
        }

        private void ShowZone()
        {
            winEffect.Hide();
            currentConfig = wheels.For(session.ZoneType);
            currentSlices = WheelRoller.Roll(currentConfig, session.Zone, random);
            wheelView.Show(currentConfig, currentSlices);
            spinFlow.SetSlices(currentSlices, currentConfig.name);
            if (showIntro) hud.ShowIntro(session.Zone);
            else hud.ShowZone(session.Zone, session.ZoneType);
        }

        private void OnSpinStarted(SpinStartedEvent evt)
        {
            if (!showIntro)
            {
                return;
            }
            showIntro = false;
            hud.ShowZone(session.Zone, session.ZoneType);
        }

        private void OnZoneChanged(ZoneChangedEvent evt) => ShowZone();

        private void OnLeaveRequested(LeaveRequestedEvent evt)
        {
            if (!session.CanLeave) return;

            session.Leave();
        }

        private void OnRestartRequested(RestartRequestedEvent evt)
        {
            showIntro = true;
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

            if (controls == null || wheelView == null || spinAnimator == null
                || resultPanel == null || hud == null || rewardHud == null || winEffect == null || rewardFly == null
                || walletView == null)
                Debug.LogWarning($"{name}: a button or view was not found.", this);
        }
#endif
    }
}