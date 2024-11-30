namespace Terminal.Constants
{
    /// <summary>
    /// A <see langword="static"/> collection of constants that contain values relevant to config files.
    /// </summary>
    public static class ConfigFileConstants
    {
        /// <summary>
        /// The extension for config files.
        /// </summary>
        public const string ConfigFileExtension = "conf";

        /// <summary>
        /// The name of the color config file, without an extension.
        /// </summary>
        public const string ColorConfigName = "color";

        /// <summary>
        /// The name of the user config file, without an extension.
        /// </summary>
        public const string UserConfigName = "user";

        /// <summary>
        /// The name of the display config file, without an extension.
        /// </summary>
        public const string DisplayConfigName = "display";

        /// <summary>
        /// The full name of the color config file.
        /// </summary>
        public const string ColorConfigFileName = $"{ColorConfigName}.{ConfigFileExtension}";

        /// <summary>
        /// The full name of the user config file.
        /// </summary>
        public const string UserConfigFileName = $"{UserConfigName}.{ConfigFileExtension}";

        /// <summary>
        /// The full name of the display config file.
        /// </summary>
        public const string DisplayConfigFileName = $"{DisplayConfigName}.{ConfigFileExtension}";
    }
}
