using TMPro;
using UnityEngine;

namespace VertigoDemo.UI
{
    public class HudView : MonoBehaviour
    {
        private const string ZoneTextName = "ui_text_zone_value";
        private const string TitleTextName = "ui_text_wheel_title_value";
        private const string InfoTextName = "ui_text_wheel_info_value";

        [SerializeField, HideInInspector] private TMP_Text zoneText;
        [SerializeField, HideInInspector] private TMP_Text titleText;
        [SerializeField, HideInInspector] private TMP_Text infoText;

        public void ShowZone(int zone, string title, string info)
        {
            zoneText.text = $"ZONE {zone}";
            titleText.text = title;
            infoText.text = info;
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
            zoneText.text = $"ZONE {zone}";
            titleText.text = "SPIN TO WIN! REWARDS GROW EVERY ZONE";
            infoText.text = "<size=85%>Avoid the bomb! Every 5th zone is safe, every 30th is golden. Leave after any spin to keep it all.</size>";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            zoneText = transform.FindDeepComponent<TMP_Text>(ZoneTextName);
            titleText = transform.FindDeepComponent<TMP_Text>(TitleTextName);
            infoText = transform.FindDeepComponent<TMP_Text>(InfoTextName);
        }
#endif
    }
}