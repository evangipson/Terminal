using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Terminal.Services
{
    /// <summary>
    /// A service that is intended to be created on a per-component basis, which manages a <see cref="Timer"/> that can wait.
    /// </summary>
    public class TimerService
    {
        private readonly Random _random = new();
        private readonly Timer _timer;

        /// <summary>
        /// Creates a new <see cref="TimerService"/> that will invoke the provided <paramref name="tickAction"/> in the
        /// provided amount of <paramref name="milliseconds"/>.
        /// </summary>
        /// <param name="tickAction">
        /// The <see cref="Action"/> to be invoked when the provided <paramref name="milliseconds"/> have elapsed.
        /// </param>
        /// <param name="milliseconds">
        /// The amount of milliseconds to wait before invoking the provided <paramref name="tickAction"/>.
        /// </param>
        public TimerService(Action<object, EventArgs> tickAction, double milliseconds = 100)
        {
            _timer = new();
            _timer.Elapsed += new ElapsedEventHandler(tickAction);
            _timer.Interval = milliseconds;
            _timer.Start();
        }

        /// <summary>
        /// Waits anywhere from 500 to 2000 milliseconds.
        /// </summary>
        public void RandomWait() => Wait(_random.Next(500, 2000));

        /// <summary>
        /// Waits for the provided amount of <paramref name="milliseconds"/>.
        /// </summary>
        /// <param name="milliseconds">
        /// The amount of milliseconds to wait.
        /// </param>
        public void Wait(double milliseconds) => _timer.Interval = milliseconds;

        /// <summary>
        /// Stops the <see cref="Timer"/>.
        /// </summary>
        public void Done() => _timer.Stop();
    }
}
