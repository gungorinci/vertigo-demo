using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace VertigoDemo.UI
{
    public class HudViewMono : MonoBehaviour
    {
        private const string ZONE_TEXT_NAME = "ui_text_zone_value";
        private const string TITLE_TEXT_NAME = "ui_text_wheel_title_value";
        private const string INFO_TEXT_NAME = "ui_text_wheel_info_value";

        [SerializeField, HideInInspector, FormerlySerializedAs("zoneText")] private TMP_Text _zoneText;
        [SerializeField, HideInInspector, FormerlySerializedAs("titleText")] private TMP_Text _titleText;
        [SerializeField, HideInInspector, FormerlySerializedAs("infoText")] private TMP_Text _infoText;

        #region Public API

        public void ShowZone(int zone, string title, string info)
        {
            _zoneText.text = string.Format(UiTexts.ZONE_FORMAT, zone);
            _titleText.text = title;
            _infoText.text = info;
        }

        public void ShowCountdown(int zone, string title, string nextZoneName, int spinsLeft)
        {
            string info = string.Format(UiTexts.COUNTDOWN_FORMAT, nextZoneName, spinsLeft);
            if (spinsLeft == 1)
            {
                info = string.Format(UiTexts.COUNTDOWN_NEXT_FORMAT, nextZoneName);
            }
            ShowZone(zone, title, info);
        }

        // Shown before the first spin of a run: the rules in one line.
        public void ShowIntro(int zone)
        {
            _zoneText.text = string.Format(UiTexts.ZONE_FORMAT, zone);
            _titleText.text = UiTexts.INTRO_TITLE;
            _infoText.text = UiTexts.INTRO_INFO;
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            _zoneText = transform.FindDeepComponent<TMP_Text>(ZONE_TEXT_NAME);
            _titleText = transform.FindDeepComponent<TMP_Text>(TITLE_TEXT_NAME);
            _infoText = transform.FindDeepComponent<TMP_Text>(INFO_TEXT_NAME);
        }
#endif
    }
}