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
            _zoneText.text = $"ZONE {zone}";
            _titleText.text = title;
            _infoText.text = info;
        }

        public void ShowCountdown(int zone, string title, string nextZoneName, int spinsLeft)
        {
            string info = $"{nextZoneName} zone in {spinsLeft} spins";
            if (spinsLeft == 1)
            {
                info = $"{nextZoneName} zone next!";
            }
            ShowZone(zone, title, info);
        }

        // Shown before the first spin of a run: the rules in one line.
        public void ShowIntro(int zone)
        {
            _zoneText.text = $"ZONE {zone}";
            _titleText.text = "SPIN TO WIN! REWARDS GROW EVERY ZONE";
            _infoText.text = "<size=85%>Avoid the bomb! Every 5th zone is safe, every 30th is golden. Leave after any spin to keep it all.</size>";
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