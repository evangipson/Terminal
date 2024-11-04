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
		private readonly double _autoScrollSpeed = 0.2;
		
		private ScreenNavigator _screenNavigator;
		private TimerService _timerService;
		private PersistService _persistService;
		private StyleBoxFlat _flatStyleBox;
		private Color _currentColor;
		private Tween _tween;
		private double _loadedValue = 0;

		public override void _Ready()
		{
			_screenNavigator = GetNode<ScreenNavigator>("/root/ScreenNavigator");
			_persistService = GetNode<PersistService>("/root/PersistService");
			_flatStyleBox = new()
			{
				BgColor = _persistService.CurrentColor,
				BorderColor = new("#000000")
			};
			AddThemeStyleboxOverride("fill", _flatStyleBox);
			
			_timerService = new(FillProgressBar);
		}

		public void FillProgressBar(object sender, EventArgs args)
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

		public void IncreaseProgressBarValue(int value)
		{
			_loadedValue += value;
			_tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
			_tween.TweenProperty(this, "value", _loadedValue, _autoScrollSpeed);
		}

		private void OnLoadingBarDone()
		{
			_timerService.Done();
			_screenNavigator.GotoScene(ScreenConstants.WelcomeScenePath);
		}
	}
}
