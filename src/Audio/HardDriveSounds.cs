using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

using Terminal.Constants;
using Terminal.Extensions;
using Terminal.Services;

namespace Terminal.Audio
{
    /// <summary>
    /// An <see cref="AudioStreamPlayer"/> <see cref="Node"/> managed in Godot that plays hard drive sounds.
    /// </summary>
    public partial class HardDriveSounds : AudioStreamPlayer
    {
        /// <summary>
        /// The absolute path for the <see cref="HardDriveSounds"/> node in Godot.
        /// </summary>
        public const string AbsolutePath = "/root/Root/HardDriveSounds";

        private readonly Random _random = new(DateTime.UtcNow.GetHashCode());

        private readonly int _maxVolumeDb = -5;
        private readonly int _minVolumeDb = -15;

        private ConfigService _configService;

        public override void _Ready()
        {
            _configService = GetNode<ConfigService>(ServicePathConstants.ConfigServicePath);
            SetVolumeFromUserConfig();
        }

        /// <summary>
        /// Plays a hard drive sound, using the user.conf defined volume.
        /// <para>
        /// Will not play a sound if the volume is at the minimum threshold.
        /// </para>
        /// </summary>
        public void PlayHardDriveSound()
        {
            if (VolumeDb != GetVolumeFromUserConfig())
            {
                SetVolumeFromUserConfig();
            }

            if (VolumeDb == _minVolumeDb)
            {
                return;
            }

            Play();
        }

        /// <summary>
        /// Plays a series of hard drive sounds.
        /// <para>
        /// Will not play any sounds if the volume is at the minimum threshold.
        /// </para>
        /// </summary>
        /// <param name="minSounds">
        /// The minimum number of hard drives sounds to be played.
        /// </param>
        /// <param name="maxSounds">
        /// The maximum number of hard drives sounds to be played.
        /// </param>
        /// <param name="minTime">
        /// The minimum time, in milliseconds, to wait between hard drive sounds.
        /// </param>
        /// <param name="maxTime">
        /// The maximum time, in milliseconds, to wait between hard drive sounds.
        /// </param>
        public async Task PlayHardDriveSounds(int minSounds = 1, int maxSounds = 4, int minTime = 50, int maxTime = 200)
        {
            var amountOfSounds = _random.Next(minSounds, maxSounds);
            foreach (var _ in Enumerable.Range(0, amountOfSounds))
            {
                var millisecondsBetweenSounds = _random.Next(minTime, maxTime);
                PlayHardDriveSound();
                await Task.Delay(millisecondsBetweenSounds);
            }
        }

        private void SetVolumeFromUserConfig()
        {
            VolumeDb = GetVolumeFromUserConfig();
        }

        private int GetVolumeFromUserConfig()
        {
            var userVolume = _configService.Volume;
            var newVolumeDb = userVolume.ConvertRange(0, 100, _minVolumeDb, _maxVolumeDb);
            return Math.Clamp(newVolumeDb, _minVolumeDb, _maxVolumeDb);
        }
    }
}
