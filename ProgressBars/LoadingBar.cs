using System;
using Godot;
using Terminal.Constants;
using Terminal.Navigators;
using Terminal.Services;

namespace Terminal.Game.ProgressBars
{
	public partial class LoadingBar : ProgressBar
	{
		private readonly Random _random = new();
		private readonly double _autoScrollSpeed = 0.3;

		private TimerService _timerService;
		private ScreenNavigator _screenNavigator;
		private Tween _tween;
		private double _loadedValue = 0;

		public override void _Ready()
		{
			_screenNavigator = GetNode<ScreenNavigator>("/root/ScreenNavigator");
			_timerService = GetNode<TimerService>("/root/TimerService");
			_timerService.OnTick += FillProgressBar;
		}

		public void FillProgressBar()
		{
			_timerService.RandomWait();
			IncreaseProgressBarValue(_random.Next(5, 20));
			if (Value >= 100)
			{
				_timerService.OnDone += GoToConsoleScreen;
			}
		}

		public void IncreaseProgressBarValue(int value)
		{
			_loadedValue += value;
			_tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
			_tween.TweenProperty(this, "value", _loadedValue, _autoScrollSpeed);
		}

		public void GoToConsoleScreen()
		{
			_screenNavigator.GotoScene(ScreenConstants.ConsoleScenePath);
		}
	}
}
