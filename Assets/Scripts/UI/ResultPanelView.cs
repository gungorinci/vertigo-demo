using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoDemo.Core;

namespace VertigoDemo.UI
{
    public class ResultPanelView : MonoBehaviour
    {
        private const string PopupName = "ui_animator_popup_result";
        private const string IconName = "ui_image_result_icon_value";
        private const string TitleName = "ui_text_result_title_value";
        private const string RestartButtonName = "ui_button_restart";
        private const string RestartTextName = "ui_text_restart_value";
        private const string LOSE_TITLE = "BOMB! ALL REWARDS LOST";
        private const string LOSE_BUTTON = "RESTART";
        private const string WIN_TITLE = "REWARDS COLLECTED!";
        private const string WIN_BUTTON = "PLAY AGAIN";

        [SerializeField, HideInInspector] private RectTransform popup;
        [SerializeField, HideInInspector] private Image icon;
        [SerializeField, HideInInspector] private TMP_Text title;
        [SerializeField, HideInInspector] private Button restartButton;
        [SerializeField, HideInInspector] private TMP_Text restartText;

        [SerializeField] private Sprite _loseIcon;
        [SerializeField] private Sprite _winIcon;
        [SerializeField, Min(0f)] private float popDuration = 0.3f;

        private IEventBus _bus;

        public void Bind(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<SessionStateChangedEvent>(OnStateChanged);
        }

        private void OnEnable() => restartButton.onClick.AddListener(OnRestartClicked);

        private void OnDisable() => restartButton.onClick.RemoveListener(OnRestartClicked);

        private void OnDestroy()
        {
            if (_bus != null)
            {
                _bus.Unsubscribe<SessionStateChangedEvent>(OnStateChanged);
            }
        }

        private void OnStateChanged(SessionStateChangedEvent evt)
        {
            if (evt.State == SessionState.Lost)
            {
                Show(_loseIcon, LOSE_TITLE, LOSE_BUTTON);
            }
            else if (evt.State == SessionState.Left)
            {
                Show(_winIcon, WIN_TITLE, WIN_BUTTON);
            }
            else if (evt.State == SessionState.Idle)
            {
                Hide();
            }
        }

        private void Show(Sprite iconSprite, string titleText, string buttonText)
        {
            icon.sprite = iconSprite;
            title.text = titleText;
            restartText.text = buttonText;

            gameObject.SetActive(true);
            popup.localScale = Vector3.zero;
            popup.DOScale(1f, popDuration).SetEase(Ease.OutBack).SetLink(gameObject);
        }

        private void Hide() => gameObject.SetActive(false);

        private void OnRestartClicked()
        {
            _bus.Publish(new RestartRequestedEvent());
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            popup = transform.FindDeep(PopupName) as RectTransform;
            icon = FindComponent<Image>(IconName);
            title = FindComponent<TMP_Text>(TitleName);
            restartButton = FindComponent<Button>(RestartButtonName);
            restartText = FindComponent<TMP_Text>(RestartTextName);
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