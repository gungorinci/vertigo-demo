using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VertigoDemo.Core;

namespace VertigoDemo.UI
{
    public class ResultPanelViewMono : MonoBehaviour
    {
        private const string POPUP_NAME = "ui_animator_popup_result";
        private const string ICON_NAME = "ui_image_result_icon_value";
        private const string TITLE_NAME = "ui_text_result_title_value";
        private const string RESTART_BUTTON_NAME = "ui_button_restart";
        private const string RESTART_TEXT_NAME = "ui_text_restart_value";

        [SerializeField, HideInInspector, FormerlySerializedAs("popup")] private RectTransform _popup;
        [SerializeField, HideInInspector, FormerlySerializedAs("icon")] private Image _icon;
        [SerializeField, HideInInspector, FormerlySerializedAs("title")] private TMP_Text _title;
        [SerializeField, HideInInspector, FormerlySerializedAs("restartButton")] private Button _restartButton;
        [SerializeField, HideInInspector, FormerlySerializedAs("restartText")] private TMP_Text _restartText;

        [SerializeField] private Sprite _loseIcon;
        [SerializeField] private Sprite _winIcon;
        [SerializeField, Min(0f), FormerlySerializedAs("popDuration")] private float _popDuration = 0.3f;

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
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(OnRestartClicked);
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

        private void OnStateChanged(SessionStateChangedEvent evt)
        {
            if (evt.State == SessionState.Lost)
            {
                Show(_loseIcon, UiTexts.LOSE_TITLE, UiTexts.LOSE_BUTTON);
            }
            else if (evt.State == SessionState.Left)
            {
                Show(_winIcon, UiTexts.WIN_TITLE, UiTexts.WIN_BUTTON);
            }
            else if (evt.State == SessionState.Idle)
            {
                Hide();
            }
        }

        private void OnRestartClicked()
        {
            _bus.Publish(new RestartRequestedEvent());
        }

        #endregion

        #region Popup

        private void Show(Sprite iconSprite, string titleText, string buttonText)
        {
            _icon.sprite = iconSprite;
            _title.text = titleText;
            _restartText.text = buttonText;

            gameObject.SetActive(true);
            _popup.localScale = Vector3.zero;
            _popup.DOScale(1f, _popDuration).SetEase(Ease.OutBack).SetLink(gameObject);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _popup = transform.FindDeep(POPUP_NAME) as RectTransform;
            _icon = FindComponent<Image>(ICON_NAME);
            _title = FindComponent<TMP_Text>(TITLE_NAME);
            _restartButton = FindComponent<Button>(RESTART_BUTTON_NAME);
            _restartText = FindComponent<TMP_Text>(RESTART_TEXT_NAME);
        }

        private T FindComponent<T>(string childName) where T : Component
        {
            Transform child = transform.FindDeep(childName);
            if (!child)
            {
                Debug.LogWarning($"{name}: child '{childName}' not found.", this);
                return null;
            }
            return child.GetComponent<T>();
        }
#endif
    }
}