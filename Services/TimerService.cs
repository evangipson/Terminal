using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Terminal.Services
{
    public class TimerService
    {
        private readonly Random _random = new();
        private readonly Timer _timer;

        public TimerService(Action<object, EventArgs> tickAction, double milliseconds = 100)
        {
            _timer = new();
            _timer.Elapsed += new ElapsedEventHandler(tickAction);
            _timer.Interval = milliseconds;
            _timer.Start();
        }

        public void RandomWait() => Wait(_random.Next(500, 2000));

        public void Wait(double milliseconds) => _timer.Interval = milliseconds;

        public void Done() => _timer.Stop();
    }
}
