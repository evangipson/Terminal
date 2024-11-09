using Terminal.Navigators;
using Terminal.Services;

namespace Terminal.Constants
{
    /// <summary>
    /// A <see langword="static"/> collection of constant values that point to singletons managed in Godot.
    /// </summary>
    public static class ServicePathConstants
    {
        /// <summary>
        /// The <see cref="PersistService"/> singleton path in Godot.
        /// </summary>
        public const string PersistServicePath = "/root/PersistService";

        /// <summary>
        /// The <see cref="ScreenNavigator"/> singleton path in Godot.
        /// </summary>
        public const string ScreenNavigatorServicePath = "/root/ScreenNavigator";
    }
}
