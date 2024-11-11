using Godot;

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

        /// <summary>
        /// Plays a keyboard sound.
        /// </summary>
        public void PlayKeyboardSound() => Play();
    }
}