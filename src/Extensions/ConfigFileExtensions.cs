using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Extensions
{
    /// <summary>
    /// A <see langword="static"/> collection of extension methods for managing config files.
    /// </summary>
    public static class ConfigFileExtensions
    {
        /// <summary>
        /// Gets a clamped integer config value associated with the provided <paramref name="configKey"/>.
        /// <para>
        /// The minimum value will be <paramref name="minValue"/>, and the maximum value will be <paramref name="defaultValue"/>.
        /// </para>
        /// </summary>
        /// <param name="targetConfig">
        /// A <see cref="Dictionary{TKey, TValue}"/> filled with <see langword="string"/> keys and values, which represents a config file's contents.
        /// </param>
        /// <param name="configKey">
        /// The key of the desired config from the config file.
        /// </param>
        /// <param name="defaultValue">
        /// A default value to return when unable to parse the config file. Also used as the maximum value for clamping the config value.
        /// <para>
        /// Defaults to <c>100</c>.
        /// </para>
        /// </param>
        /// <param name="minValue">
        /// The minimum value for clamping the config value.
        /// <para>
        /// Defaults to <c>0</c>.
        /// </para>
        /// </param>
        /// <returns>
        /// An <see langword="int"/> value from the <paramref name="targetConfig"/>, clamped between <paramref name="minValue"/> and <paramref name="defaultValue"/>.
        /// </returns>
        public static int GetLatestIntegerConfig(this Dictionary<string, string> targetConfig, string configKey, int defaultValue = 100, int minValue = 0)
        {
            if (targetConfig == null)
            {
                return defaultValue;
            }

            var configKeyValue = targetConfig
                .Where(config => config.Key.Equals(configKey, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(config =>
                {
                    if (!int.TryParse(config.Value, out int configValue))
                    {
                        return false;
                    }

                    return true;
                });

            if (configKeyValue.Key == default || configKeyValue.Value == default)
            {
                return defaultValue;
            }

            return Math.Clamp(int.Parse(configKeyValue.Value), minValue, defaultValue);
        }
    }
}
