using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class GameController : MonoBehaviour
    {
        private const string SpinButtonName = "ui_button_spin_wheel";
        private const string LeaveButtonName = "ui_button_leave";

        [SerializeField] private WheelConfig normalWheel;   // bronze, with the bomb
        [SerializeField] private WheelConfig safeWheel;     // silver, every 5th zone
        [SerializeField] private WheelConfig superWheel;    // gold, every 30th zone

        [Header("Result panel")]
        [SerializeField] private Sprite loseIcon;
        [SerializeField] private Sprite winIcon;
        [SerializeField, Min(0f)] private float resultDelay = 1f;
        [SerializeField, Min(0f)] private float flyDelay = 0.4f;

        [SerializeField, HideInInspector] private Button spinButton;
        [SerializeField, HideInInspector] private Button leaveButton;
        [SerializeField, HideInInspector] private WheelView wheelView;
        [SerializeField, HideInInspector] private WheelSpinAnimator spinAnimator;
        [SerializeField, HideInInspector] private ResultPanelView resultPanel;
        [SerializeField, HideInInspector] private HudView hud;
        [SerializeField, HideInInspector] private RewardHudView rewardHud;
        [SerializeField, HideInInspector] private WheelWinEffect winEffect;
        [SerializeField, HideInInspector] private RewardFlyEffect rewardFly;

        private readonly IRandomSource random = new SystemRandomSource();
        private List<RolledSlice> currentSlices;
        private GameSession<RewardData> session;
        private WheelConfig currentConfig;

        private void OnEnable()
        {
            spinButton.onClick.AddListener(OnSpinClicked);
            leaveButton.onClick.AddListener(OnLeaveClicked);
            resultPanel.RestartClicked += OnRestartClicked;
        }

        private void OnDisable()
        {
            spinButton.onClick.RemoveListener(OnSpinClicked);
            leaveButton.onClick.RemoveListener(OnLeaveClicked);
            resultPanel.RestartClicked -= OnRestartClicked;
        }

        private void Start()
        {
            session = new GameSession<RewardData>();
            session.ZoneChanged += OnZoneChanged;
            session.RewardChanged += OnRewardChanged;
            session.StateChanged += OnStateChanged;
            session.RewardsCleared += OnRewardsCleared;
            ShowZone();
            UpdateButtons();
        }

        private void OnDestroy()
        {
            if (session == null) return;
            session.ZoneChanged -= OnZoneChanged;
            session.RewardChanged -= OnRewardChanged;
            session.StateChanged -= OnStateChanged;
            session.RewardsCleared -= OnRewardsCleared;
        }

        private void ShowZone()
        {
            winEffect.Hide();
            currentConfig = GetConfig(session.ZoneType);
            currentSlices = WheelRoller.Roll(currentConfig, random);
            wheelView.Show(currentConfig, currentSlices);
            hud.ShowZone(session.Zone, session.ZoneType);
        }

        private WheelConfig GetConfig(ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Safe: return safeWheel;
                case ZoneType.Super: return superWheel;
                default: return normalWheel;
            }
        }

        private void OnSpinClicked()
        {
            if (!session.CanSpin) return;

            var weights = new List<int>(currentSlices.Count);
            foreach (RolledSlice slice in currentSlices) weights.Add(slice.Weight);

            int winner = WeightedPicker.Pick(weights, random);
            session.BeginSpin();
            spinAnimator.Spin(winner, currentSlices.Count, random, () => OnSpinFinished(winner));
        }

        private void OnSpinFinished(int winner)
        {
            RolledSlice slice = currentSlices[winner];
            winEffect.Play(winner);

            if (slice.Reward.IsBomb)
            {
                DOVirtual.DelayedCall(resultDelay, () => CompleteSpin(slice)).SetLink(gameObject);
                return;
            }

            DOVirtual.DelayedCall(flyDelay, () => rewardFly.Fly(
                    slice.Reward.Icon,
                    wheelView.GetSliceIcon(winner),
                    rewardHud.GetTargetPosition(slice.Reward),
                    () => CompleteSpin(slice)))
                .SetLink(gameObject);
        }

        private void CompleteSpin(RolledSlice slice) =>
            session.CompleteSpin(slice.Reward, slice.Amount, slice.Reward.IsBomb);

        private void OnZoneChanged(int zone) => ShowZone();

        private void OnRewardChanged(RewardData reward, int total) => rewardHud.SetReward(reward, total);

        private void OnRewardsCleared() => rewardHud.Clear();

        private void OnStateChanged(SessionState state)
        {
            UpdateButtons();

            switch (state)
            {
                case SessionState.Lost:
                    resultPanel.Show(loseIcon, "BOMB! ALL REWARDS LOST", "RESTART");
                    break;
                case SessionState.Left:
                    resultPanel.Show(winIcon, "REWARDS COLLECTED!", "PLAY AGAIN");
                    break;
                case SessionState.Idle:
                    resultPanel.Hide();
                    break;
            }
        }

        private void UpdateButtons()
        {
            spinButton.interactable = session.CanSpin;
            leaveButton.interactable = session.CanLeave;
        }

        private void OnLeaveClicked()
        {
            if (session.CanLeave) session.Leave();
        }

        private void OnRestartClicked() => session.Restart();

#if UNITY_EDITOR
        private void OnValidate()
        {
            Transform button = transform.FindDeep(SpinButtonName);
            spinButton = button != null ? button.GetComponent<Button>() : null;
            wheelView = GetComponentInChildren<WheelView>(true);
            spinAnimator = GetComponentInChildren<WheelSpinAnimator>(true);

            Transform leave = transform.FindDeep(LeaveButtonName);
            leaveButton = leave != null ? leave.GetComponent<Button>() : null;
            resultPanel = GetComponentInChildren<ResultPanelView>(true);
            hud = GetComponentInChildren<HudView>(true);
            rewardHud = GetComponentInChildren<RewardHudView>(true);
            winEffect = GetComponentInChildren<WheelWinEffect>(true);
            rewardFly = GetComponentInChildren<RewardFlyEffect>(true);

            if (spinButton == null || leaveButton == null || wheelView == null || spinAnimator == null
                || resultPanel == null || hud == null || rewardHud == null || winEffect == null || rewardFly == null)
                Debug.LogWarning($"{name}: a button or view was not found.", this);
        }
#endif
    }
}