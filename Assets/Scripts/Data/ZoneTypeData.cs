using UnityEngine;
using VertigoDemo.Core;

namespace VertigoDemo.Data
{
    [CreateAssetMenu(fileName = "Zone_", menuName = "Vertigo Demo/Zone Type")]
    public class ZoneTypeData : ScriptableObject, IZoneType
    {
        [SerializeField, Min(1), Tooltip("Every Nth zone is this type. 1 is the default type of every other zone.")]
        private int _interval = 1;
        [SerializeField] private bool _hasBomb;
        [SerializeField] private WheelConfig _wheel;
        [SerializeField] private string _displayName;
        [SerializeField] private string _title;
        [SerializeField, Tooltip("Leave empty to show the spins left until the next special zone.")]
        private string _info;

        public int Interval
        {
            get { return _interval; }
        }

        public bool HasBomb
        {
            get { return _hasBomb; }
        }

        public WheelConfig Wheel
        {
            get { return _wheel; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string Title
        {
            get { return _title; }
        }

        public string Info
        {
            get { return _info; }
        }

        public bool HasInfo
        {
            get { return !string.IsNullOrEmpty(_info); }
        }

        #region Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_wheel)
            {
                Debug.LogError($"{name}: no wheel is assigned.", this);
                return;
            }

            int expectedBombSlices = _hasBomb ? 1 : 0;
            int bombSlices = _wheel.CountBombSlices();
            if (bombSlices != expectedBombSlices)
            {
                Debug.LogError($"{name}: needs {expectedBombSlices} bomb slice(s), {_wheel.name} has {bombSlices}.", this);
            }
        }
#endif

        #endregion
    }
}
