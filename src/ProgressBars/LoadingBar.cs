using System;
using Godot;

using Terminal.Audio;
using Terminal.Constants;
using Terminal.Navigators;
using Terminal.Services;

namespace Terminal.Game.ProgressBars
{
    /// <summary>
    /// A <see cref="ProgressBar"/> <see cref="Node"/> managed in Godot that shows when the game is initially booting.
    /// </summary>
    public partial class LoadingBar : ProgressBar
    {
        private readonly Random _random = new(DateTime.UtcNow.GetHashCode());
        private readonly double _autoScrollSpeed = 0.2;

        private ScreenNavigator _screenNavigator;
        private TimerService _timerService;
        private PersistService _persistService;
        private HardDriveSounds _hardDriveSounds;
        private StyleBoxFlat _flatStyleBox;
        private Tween _tween;
        private double _loadedValue = 0;

        public override void _Draw()
        {
            if (_screenNavigator == null)
            {
                _screenNavigator = GetNode<ScreenNavigator>(ServicePathConstants.ScreenNavigatorServicePath);
                _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
                _hardDriveSounds = GetTree().Root.GetNode<HardDriveSounds>(HardDriveSounds.AbsolutePath);
                _flatStyleBox = new()
                {
                    BgColor = _persistService.CurrentColor,
                    BorderColor = ColorConstants.TerminalBlack
                };
                AddThemeStyleboxOverride("fill", _flatStyleBox);
                _timerService = new(FillProgressBar);
            }
        }

        private void FillProgressBar(object sender, EventArgs args)
        {
            _timerService.RandomWait();
            var increasedProgressBarValue = _random.Next(5, 20);
            if (Value >= 100)
            {
                CallDeferred("OnLoadingBarDone");
                return;
            }
            CallDeferred("IncreaseProgressBarValue", increasedProgressBarValue);
        }

        private void IncreaseProgressBarValue(int value)
        {
            _loadedValue += value;
            _tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
            _tween.TweenProperty(this, "value", _loadedValue, _autoScrollSpeed);

            _ = _hardDriveSounds.PlayHardDriveSounds();
        }

        private void OnLoadingBarDone()
        {
            _timerService.Done();
            _screenNavigator.GotoScene(ScenePathConstants.WelcomeScenePath);
        }
    }
}
