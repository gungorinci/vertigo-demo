using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VertigoDemo.UI
{
    public class MainMenuView : MonoBehaviour
    {
        private const string PlayButtonName = "ui_button_play";
        private const string GameSceneName = "Wheel";

        [SerializeField, HideInInspector] private Button playButton;

        private void OnEnable() => playButton.onClick.AddListener(OnPlayClicked);

        private void OnDisable() => playButton.onClick.RemoveListener(OnPlayClicked);

        private void OnPlayClicked() => SceneManager.LoadScene(GameSceneName);

#if UNITY_EDITOR
        private void OnValidate()
        {
            Transform child = transform.FindDeep(PlayButtonName);
            playButton = child != null ? child.GetComponent<Button>() : null;

            if (playButton == null)
                Debug.LogWarning($"{name}: child '{PlayButtonName}' with a Button not found.", this);
        }
#endif
    }
}
