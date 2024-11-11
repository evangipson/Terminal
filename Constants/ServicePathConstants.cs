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
        /// The <see cref="ConfigService"/> singleton path in Godot.
        /// </summary>
        public const string ConfigServicePath = "/root/ConfigService";

        /// <summary>
        /// The <see cref="PersistService"/> singleton path in Godot.
        /// </summary>
        public const string PersistServicePath = "/root/PersistService";

        /// <summary>
        /// The <see cref="UserCommandService"/> singleton path in Godot.
        /// </summary>
        public const string UserCommandServicePath = "/root/UserCommandService";

        /// <summary>
        /// The <see cref="DirectoryService"/> singleton path in Godot.
        /// </summary>
        public const string DirectoryServicePath = "/root/DirectoryService";

        /// <summary>
        /// The <see cref="ScreenNavigator"/> singleton path in Godot.
        /// </summary>
        public const string ScreenNavigatorServicePath = "/root/ScreenNavigator";

        /// <summary>
        /// The <see cref="AutoCompleteService"/> single path in Godot.
        /// </summary>
        public const string AutoCompleteServicePath = "/root/AutoCompleteService";
    }
}
