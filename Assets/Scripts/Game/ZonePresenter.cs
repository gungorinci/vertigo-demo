using System;
using System.Collections.Generic;
using VertigoDemo.Core;
using VertigoDemo.Data;
using VertigoDemo.UI;

namespace VertigoDemo.Game
{
    public class ZonePresenter : IDisposable
    {
        private const int FIRST_ZONE = 1;

        private readonly IEventBus _bus;
        private readonly GameSession<RewardData> _session;
        private readonly ZoneSchedule<ZoneTypeData> _schedule;
        private readonly IRandomSource _random;
        private readonly SpinFlow _spinFlow;
        private readonly WheelView _wheelView;
        private readonly WheelWinEffect _winEffect;
        private readonly HudView _hud;

        private bool _isIntroVisible;

        public ZonePresenter(
            IEventBus bus,
            GameSession<RewardData> session,
            ZoneSchedule<ZoneTypeData> schedule,
            IRandomSource random,
            SpinFlow spinFlow,
            WheelView wheelView,
            WheelWinEffect winEffect,
            HudView hud)
        {
            _bus = bus;
            _session = session;
            _schedule = schedule;
            _random = random;
            _spinFlow = spinFlow;
            _wheelView = wheelView;
            _winEffect = winEffect;
            _hud = hud;

            _bus.Subscribe<ZoneChangedEvent>(OnZoneChanged);
            _bus.Subscribe<SpinStartedEvent>(OnSpinStarted);
        }

        #region Public API

        public void ShowZone()
        {
            _winEffect.Hide();

            WheelConfig config = _schedule.For(_session.Zone).Wheel;
            List<RolledSlice> slices = WheelRoller.Roll(config, _session.Zone, _random);
            _wheelView.Show(config, slices);
            _spinFlow.SetSlices(slices, config.name);

            // The rules are shown until the first spin of a run, and every run starts at zone 1.
            _isIntroVisible = _session.Zone == FIRST_ZONE;
            if (_isIntroVisible)
            {
                _hud.ShowIntro(_session.Zone);
            }
            else
            {
                ShowHud();
            }
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ZoneChangedEvent>(OnZoneChanged);
            _bus.Unsubscribe<SpinStartedEvent>(OnSpinStarted);
        }

        #endregion

        #region Handlers

        private void OnZoneChanged(ZoneChangedEvent evt)
        {
            ShowZone();
        }

        private void OnSpinStarted(SpinStartedEvent evt)
        {
            if (!_isIntroVisible)
            {
                return;
            }
            _isIntroVisible = false;
            ShowHud();
        }

        #endregion

        #region Hud

        private void ShowHud()
        {
            int zone = _session.Zone;
            ZoneTypeData type = _schedule.For(zone);
            int nextZone;
            if (type.HasInfo || !_schedule.TryFindNextSpecialZone(zone, out nextZone))
            {
                _hud.ShowZone(zone, type.Title, type.Info);
                return;
            }

            _hud.ShowCountdown(zone, type.Title, _schedule.For(nextZone).DisplayName, nextZone - zone);
        }

        #endregion
    }
}
