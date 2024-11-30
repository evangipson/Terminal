namespace Terminal.Constants
{
    /// <summary>
    /// A <see langword="static"/> collection of constants that contain values relevant to config files and their keys.
    /// </summary>
    public static class ConfigConstants
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

        /// <summary>
        /// The key for volume configuration.
        /// </summary>
        public const string VolumeConfigKey = "volume";

        /// <summary>
        /// The key for monitor shader intensity configuration.
        /// </summary>
        public const string MonitorShaderIntensityConfigKey = "monitor-effect-intensity";

        /// <summary>
        /// The key for font size configuration.
        /// </summary>
        public const string FontSizeConfigKey = "font-size";
    }
}
