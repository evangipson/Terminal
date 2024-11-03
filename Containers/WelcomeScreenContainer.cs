using System;
using Godot;
using Terminal.Constants;
using Terminal.Navigators;
using Terminal.Services;

namespace Terminal.Containers
{
	public partial class WelcomeScreenContainer : BoxContainer
	{
		private readonly Random _random = new();

		private TimerService _timerService;
		private ScreenNavigator _screenNavigator;
		private Label _welcomeLabel;
		private float _lettersShowing;

		public override void _Draw()
		{
			if(_welcomeLabel == null)
			{
				_welcomeLabel = GetNode<ColorRect>("Background").GetNode<Label>("WelcomeLabel");
				_screenNavigator = GetNode<ScreenNavigator>("/root/ScreenNavigator");
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
			CallDeferred("OnTextAnimating");
		}

		public void OnTextAnimating()
		{
			if (_lettersShowing > 1.0f)
			{
				CallDeferred("OnTextAnimated");
				return;
			}

			_lettersShowing += 0.08f;
			_welcomeLabel.VisibleRatio = _lettersShowing;
		}

		public void OnTextAnimated()
		{
			_timerService.Done();
			_screenNavigator.GotoScene(ScreenConstants.ConsoleScenePath);
		}
	}
}