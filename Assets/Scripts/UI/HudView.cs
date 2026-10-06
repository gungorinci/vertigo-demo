using TMPro;
using UnityEngine;
using VertigoDemo.Core;

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

        public void ShowZone(int zone, ZoneType type)
        {
            zoneText.text = $"ZONE {zone}";
            titleText.text = TitleFor(type);
            infoText.text = InfoFor(zone, type);
        }

        private static string TitleFor(ZoneType type) => type switch
        {
            ZoneType.Safe => "SILVER SPIN",
            ZoneType.Super => "GOLDEN SPIN",
            _ => "BRONZE SPIN"
        };

        private static string InfoFor(int zone, ZoneType type)
        {
            if (type == ZoneType.Safe) return "SAFE ZONE: no bomb, better rewards";
            if (type == ZoneType.Super) return "SUPER ZONE: no bomb, special rewards";

            int spinsLeft = ZoneRules.SafeInterval - zone % ZoneRules.SafeInterval;
            string next = ZoneRules.GetZoneType(zone + spinsLeft) == ZoneType.Super ? "Super" : "Safe";
            return spinsLeft == 1 ? $"{next} zone next!" : $"{next} zone in {spinsLeft} spins";
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