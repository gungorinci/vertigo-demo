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
        private readonly WheelSelector<WheelConfig> _wheels;
        private readonly IRandomSource _random;
        private readonly SpinFlow _spinFlow;
        private readonly WheelView _wheelView;
        private readonly WheelWinEffect _winEffect;
        private readonly HudView _hud;

        private bool _isIntroVisible;

        public ZonePresenter(
            IEventBus bus,
            GameSession<RewardData> session,
            WheelSelector<WheelConfig> wheels,
            IRandomSource random,
            SpinFlow spinFlow,
            WheelView wheelView,
            WheelWinEffect winEffect,
            HudView hud)
        {
            _bus = bus;
            _session = session;
            _wheels = wheels;
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

            WheelConfig config = _wheels.For(_session.ZoneType);
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
                _hud.ShowZone(_session.Zone, _session.ZoneType);
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
            _hud.ShowZone(_session.Zone, _session.ZoneType);
        }

        #endregion
    }
}
