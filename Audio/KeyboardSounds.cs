using Godot;

namespace Terminal.Audio
{
    public partial class KeyboardSounds : AudioStreamPlayer
    {
        public const string AbsolutePath = "/root/Root/KeyboardSounds";

        public void PlayKeyboardSound() => Play();
    }
}