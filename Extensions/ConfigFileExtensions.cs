using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Terminal.Extensions
{
    /// <summary>
    /// A <see langword="static"/> collection of extension methods for managing config files.
    /// </summary>
    public static class ConfigFileExtensions
    {
        /// <summary>
        /// Gets an integer config value from a config file.
        /// </summary>
        /// <param name="targetConfig">
        /// A <see cref="Dictionary{TKey, TValue}"/> filled with <see langword="string"/> keys and values, which represents a config file's contents.
        /// </param>
        /// <param name="defaultValue">
        /// A default value to return when unable to parse the config file.
        /// </param>
        /// <param name="configFileName">
        /// The name of the config file, used for logging.
        /// </param>
        /// <returns>
        /// An <see langword="int"/> value from the <paramref name="targetConfig"/>.
        /// </returns>
        public static int GetLatestIntegerConfig(this Dictionary<string, string> targetConfig, int defaultValue, string configFileName)
        {
            if (targetConfig == null)
            {
                GD.Print($"Attempted to parse integer-based config data, but \"{configFileName}\" file was not found.");
                return defaultValue;
            }

            var configKeyValue = targetConfig.FirstOrDefault(config =>
            {
                if (!int.TryParse(config.Value, out int configValue))
                {
                    GD.Print($"Attempted to parse integer-based config data from {configFileName}, but {config.Value} isn't a valid number.");
                    return false;
                }

                return true;
            });

            if (configKeyValue.Key == default || configKeyValue.Value == default)
            {
                return defaultValue;
            }

            return int.Parse(configKeyValue.Value);
        }
    }
}
