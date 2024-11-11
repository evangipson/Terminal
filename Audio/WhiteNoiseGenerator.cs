using System;
using Godot;

namespace Terminal.Audio
{
    /// <summary>
    /// An <see cref="AudioStreamPlayer"/> <see cref="Node"/> managed in Godot that generates white noise in real-time.
    /// </summary>
    public partial class WhiteNoiseGenerator : AudioStreamPlayer
    {
        /// <summary>
        /// The player responsible for outputting the sounds for <see cref="WhiteNoiseGenerator"/>.
        /// </summary>
        [Export]
        public AudioStreamPlayer Player { get; set; }

        private AudioStreamGeneratorPlayback _playback;
        private readonly Random _random = new();
        private Tween _tween;
        private float _sampleHz;
        private float _pulseHz = 220.0f;
        private double _elapsedTime = 0;
        private double _elapsedTonalShiftTime = 0;
        private double _timeBeforeTonalShift;
        private float _pitchScale;
        private float _amountToChangePitchScale;
        private double _pitchScaleShiftSpeed;

        public override void _Ready()
        {
            if (Player.Stream is not AudioStreamGenerator generator)
            {
                return;
            }

            _sampleHz = generator.MixRate;
            Player.Play();
            _playback = (AudioStreamGeneratorPlayback)Player.GetStreamPlayback();
            _pitchScale = PitchScale;
            FillBuffer();

            _timeBeforeTonalShift = _random.Next(20, 120);
        }

        public override void _Process(double delta)
        {
            _elapsedTime += delta;
            if (_elapsedTime >= 0.9)
            {
                FillBuffer();
                _elapsedTime = 0;
            }

            _elapsedTonalShiftTime += delta;
            if (_elapsedTonalShiftTime >= _timeBeforeTonalShift)
            {
                _amountToChangePitchScale = _random.Next(-10, 10) / 1000f;
                _pitchScale = (float)Mathf.Clamp(_pitchScale + _amountToChangePitchScale, 0.01, 0.035);
                _pitchScaleShiftSpeed = _random.Next(500, 1000) / 100f;
                _tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
                _tween.TweenProperty(this, "pitch_scale", _pitchScale, _pitchScaleShiftSpeed);

                _timeBeforeTonalShift = _random.Next(20, 120);
                _elapsedTonalShiftTime = 0;
            }
        }

        private void FillBuffer()
        {
            int availableFrames = _playback.GetFramesAvailable();
            for (int i = 0; i < availableFrames; i++)
            {
                _playback.PushFrame(new Vector2()
                {
                    X = (float)_random.NextDouble(),
                    Y = (float)_random.NextDouble()
                });
            }
        }
    }
}