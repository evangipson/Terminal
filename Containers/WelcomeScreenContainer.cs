using System;
using Godot;

using Terminal.Audio;
using Terminal.Constants;
using Terminal.Navigators;
using Terminal.Services;

namespace Terminal.Containers
{
    /// <summary>
    /// A <see cref="BoxContainer"/> <see cref="Node"/> managed in Godot that manages the welcome screen that reads "terminal_os".
    /// </summary>
    public partial class WelcomeScreenContainer : BoxContainer
    {
        private readonly Random _random = new();

        private TimerService _timerService;
        private ScreenNavigator _screenNavigator;
        private KeyboardSounds _keyboardSounds;
        private PersistService _persistService;
        private Label _welcomeLabel;
        private float _lettersShowing;

        public override void _Draw()
        {
            if (_screenNavigator == null)
            {
                _screenNavigator = GetNode<ScreenNavigator>(ServicePathConstants.ScreenNavigatorServicePath);
                _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
                _welcomeLabel = GetNode<ColorRect>("Background").GetNode<Label>("WelcomeLabel");
                _keyboardSounds = GetTree().Root.GetNode<KeyboardSounds>(KeyboardSounds.AbsolutePath);
                _welcomeLabel.AddThemeColorOverride("font_color", _persistService.CurrentColor);
                _timerService = new(AnimateText);
            }
        }

        private void AnimateText(object sender, EventArgs args)
        {
            double timeToWait = _random.Next(2000, 3000);
            if (_lettersShowing <= 0.9f)
            {
                timeToWait = _random.Next(150, 500);
            }

            _timerService.Wait(timeToWait);
            CallDeferred("PlayKeyboardSound");
            CallDeferred("OnTextAnimating");
        }

        private void OnTextAnimating()
        {
            if (_lettersShowing > 1.0f)
            {
                CallDeferred("OnTextAnimated");
                return;
            }

            _lettersShowing += 0.08f;
            _welcomeLabel.VisibleRatio = _lettersShowing;
        }

        private void OnTextAnimated()
        {
            _timerService.Done();
            _screenNavigator.GotoScene(ScenePathConstants.ConsoleScenePath);
        }

        private void PlayKeyboardSound() => _keyboardSounds.PlayKeyboardSound();
    }
}