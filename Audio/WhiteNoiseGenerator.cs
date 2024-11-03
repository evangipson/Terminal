using System;
using Godot;

public partial class WhiteNoiseGenerator : AudioStreamPlayer
{
	[Export]
	public AudioStreamPlayer Player { get; set; }

	private AudioStreamGeneratorPlayback _playback;
	private readonly Random _random = new();
	private float _sampleHz;
	private float _pulseHz = 220.0f;
	private double _elapsedTime = 0;
	private double _elapsedTonalShiftTime = 0;
	private double _timeBeforeTonalShift;
	private float _amountToChangePitchScale;

	public override void _Ready()
	{
		if (Player.Stream is AudioStreamGenerator generator)
		{
			_sampleHz = generator.MixRate;
			Player.Play();
			_playback = (AudioStreamGeneratorPlayback)Player.GetStreamPlayback();
			FillBuffer();

			_timeBeforeTonalShift = _random.Next(5, 15);
		}
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
		if (_elapsedTime >= _timeBeforeTonalShift)
		{
			_amountToChangePitchScale = _random.Next(1, 10) / 1000f;
			PitchScale += (float)Math.Clamp(_random.Next(0, 2) == 0 ? _amountToChangePitchScale : _amountToChangePitchScale * -1, 0.01, 0.06);

			_timeBeforeTonalShift = _random.Next(5, 15);
			_elapsedTonalShiftTime = 0;
		}
	}

	public void FillBuffer()
	{
		int availableFrames = _playback.GetFramesAvailable();
		for (int i = 0; i < availableFrames; i++)
		{
			_playback.PushFrame(new Vector2()
			{
				X = _random.Next(1, 100) / 100f,
				Y = _random.Next(1, 100) / 100f
			});
		}
	}
}
