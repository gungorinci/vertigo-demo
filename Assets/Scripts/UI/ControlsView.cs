using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.Core;

namespace VertigoDemo.UI
{
    public class ControlsView : MonoBehaviour
    {
        private const string SPIN_BUTTON_NAME = "ui_button_spin_wheel";
        private const string LEAVE_BUTTON_NAME = "ui_button_leave";

        [SerializeField, HideInInspector] private Button _spinButton;
        [SerializeField, HideInInspector] private Button _leaveButton;

        private IEventBus _bus;

        #region Public API

        public void Bind(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<SessionStateChangedEvent>(OnStateChanged);
        }

        #endregion

        #region Unity

        private void OnEnable()
        {
            _spinButton.onClick.AddListener(OnSpinClicked);
            _leaveButton.onClick.AddListener(OnLeaveClicked);
        }

        private void OnDisable()
        {
            _spinButton.onClick.RemoveListener(OnSpinClicked);
            _leaveButton.onClick.RemoveListener(OnLeaveClicked);
        }

        private void OnDestroy()
        {
            if (_bus != null)
            {
                _bus.Unsubscribe<SessionStateChangedEvent>(OnStateChanged);
            }
        }

        #endregion

        #region Handlers

        private void OnSpinClicked()
        {
            _bus.Publish(new SpinRequestedEvent());
        }

        private void OnLeaveClicked()
        {
            _bus.Publish(new LeaveRequestedEvent());
        }

        private void OnStateChanged(SessionStateChangedEvent evt)
        {
            _spinButton.interactable = evt.CanSpin;
            _leaveButton.interactable = evt.CanLeave;
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spinButton = transform.FindDeepComponent<Button>(SPIN_BUTTON_NAME);
            _leaveButton = transform.FindDeepComponent<Button>(LEAVE_BUTTON_NAME);
        }
#endif
    }
}
