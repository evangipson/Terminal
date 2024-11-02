using System;
using Godot;
using Timer = System.Timers.Timer;

namespace Terminal.Services
{
	public partial class TimerService : Node
	{
		public event Action OnTick;

		public event Action OnDone;

		private readonly Timer _timer = new()
		{
			Enabled = false,
			AutoReset = false
		};

		private readonly Random _random = new();

		public override void _Ready()
		{
			_timer.Elapsed += (_, _) =>
			{
				CallDeferred("TickHandler");
			};
			_timer.Start();
		}

		public void RandomWait() => Wait(_random.Next(500, 2000));

		public void Wait(double milliseconds)
		{
			_timer.Interval = milliseconds;
		}

		private void TickHandler()
		{
			if(OnDone == null)
			{
				OnTick();
				return;
			}

			DoneHandler();
		}

		private void DoneHandler()
		{
			_timer.Elapsed -= (_, _) => CallDeferred("TickHandler");
			_timer.Stop();

			OnDone();
		}
	}
}
