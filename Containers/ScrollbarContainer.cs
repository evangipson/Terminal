using Godot;

public partial class ScrollbarContainer : ScrollContainer
{
	private ScrollBar _scrollbar;
	private double _autoScrollSpeed = 0.1;
	private double _maxScrollLength = 0;
	private Tween _tween;

	public override void _Ready()
	{
		_scrollbar = GetVScrollBar();
		_scrollbar.Connect(ScrollBar.SignalName.Changed, Callable.From(OnRangeChanged));
		_maxScrollLength = _scrollbar.MaxValue;
	}

	private async void OnRangeChanged()
	{
		// wait a frame before animating anything
		await ToSignal(_scrollbar, ScrollBar.SignalName.Changed);
		AnimateScrollMax();
	}

	private void AnimateScrollMax()
	{
		if (_maxScrollLength != _scrollbar.MaxValue)
		{
			_maxScrollLength = _scrollbar.MaxValue;
			_tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
			_tween.TweenProperty(this, "scroll_vertical", _maxScrollLength - GetRect().Size.Y, _autoScrollSpeed);
		}
	}
}
