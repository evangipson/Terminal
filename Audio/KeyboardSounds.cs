using System;
using Godot;

using Terminal.Constants;
using Terminal.Extensions;
using Terminal.Services;

namespace Terminal.Audio
{
    /// <summary>
    /// An <see cref="AudioStreamPlayer"/> <see cref="Node"/> managed in Godot that plays keyboard sounds.
    /// </summary>
    public partial class KeyboardSounds : AudioStreamPlayer
    {
        /// <summary>
        /// The absolute path for the <see cref="KeyboardSounds"/> node in Godot.
        /// </summary>
        public const string AbsolutePath = "/root/Root/KeyboardSounds";

        private int _maxVolumeDb = 0;
        private int _minVolumeDb = -15;
        private ConfigService _configService;

        public override void _Ready()
        {
            _configService = GetNode<ConfigService>(ServicePathConstants.ConfigServicePath);
            SetVolumeFromUserConfig();
        }

        /// <summary>
        /// Plays a keyboard sound, using the user.conf defined volume.
        /// <para>
        /// Will not play a sound if the volume is at the minimum threshold.
        /// </para>
        /// </summary>
        public void PlayKeyboardSound()
        {
            if(VolumeDb != GetVolumeFromUserConfig())
            {
                SetVolumeFromUserConfig();
            }

            if(VolumeDb == _minVolumeDb)
            {
                return;
            }

            Play();
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