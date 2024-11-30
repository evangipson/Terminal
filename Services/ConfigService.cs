using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Extensions;

namespace Terminal.Services
{
    /// <summary>
    /// A global singleton that is responsible for parsing config (.conf) files.
    /// </summary>
    public partial class ConfigService : Node
    {
        /// <summary>
        /// An event that will be invoked when the "display.conf" config file is updated.
        /// </summary>
        public event Action<int> OnDisplayConfigChange;

        private DirectoryService _directoryService;
        private Dictionary<string, Color> _colors;
        private int? _volume;
        private int? _monitorShaderIntensity;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
        }

        private Dictionary<string, string> ColorConfig => LoadConfigFile(ConfigFileConstants.ColorConfigFileName);

        private Dictionary<string, string> UserConfig => LoadConfigFile(ConfigFileConstants.UserConfigFileName, true);

        private Dictionary<string, string> DisplayConfig => LoadConfigFile(ConfigFileConstants.DisplayConfigFileName, true);

        /// <summary>
        /// A map of color names to <see cref="Color"/>, which are supported for the console.
        /// <para>
        /// Can be modified in runtime by editing the <c>/system/config/color.conf</c> file.
        /// </para>
        /// </summary>
        public Dictionary<string, Color> Colors
        {
            get => _colors ??= GetLatestColorConfigValues();
            private set => _colors = value;
        }

        /// <summary>
        /// Represents the parsed value for the user-defined "volume" setting in the user.conf file.
        /// <para>
        /// Defaults to <c>100</c>.
        /// </para>
        /// </summary>
        public int Volume
        {
            get => _volume ??= GetLatestUserConfigVolume();
            private set => _volume = value;
        }

        /// <summary>
        /// Represents the parsed value for the user-defined "display" setting in the display.conf file.
        /// <para>
        /// Defaults to <c>100</c>.
        /// </para>
        /// </summary>
        public int MonitorShaderIntensity
        {
            get => _monitorShaderIntensity ??= GetLatestDisplayEffectConfig();
            private set => _monitorShaderIntensity = value;
        }

        /// <summary>
        /// The <see cref="Colors"/> map, but with values shifted, to show executable files as a different color.
        /// <para>
        /// Can be modified in runtime by editing the <c>/system/config/color.conf</c> file.
        /// </para>
        /// </summary>
        public Dictionary<string, Color> ExecutableColors => new(Colors.Select(color =>
        {
            var invertedColor = Colors.SkipWhile(c => c.Key != color.Key).Skip(1).FirstOrDefault();
            return new KeyValuePair<string, Color>(color.Key, invertedColor.Key == default ? Colors.First().Value : invertedColor.Value);
        }));

        /// <summary>
        /// Refreshes system and user configuration values.
        /// </summary>
        public void UpdateConfigInformation()
        {
            Colors = GetLatestColorConfigValues();
            Volume = GetLatestUserConfigVolume();
            MonitorShaderIntensity = GetLatestDisplayEffectConfig();

            OnDisplayConfigChange?.Invoke(MonitorShaderIntensity);
        }

        private Dictionary<string, string> LoadConfigFile(string fileName, bool userConfig = false)
        {
            var configFile = userConfig
                ? _directoryService.GetAbsoluteDirectory("users/user/config").FindFile(fileName)
                : _directoryService.GetAbsoluteDirectory("system/config").FindFile(fileName);

            if (configFile == null)
            {
                GD.Print($"Attempted to load the \"{fileName}\" config file, but it was not found.");
                return null;
            }

            var configFileData = configFile.Contents.Contains('\n')
                ? configFile.Contents.Split('\n').ToList()
                : new List<string>() { configFile.Contents };

            if (configFileData.Count == 0)
            {
                GD.Print($"Attempted to parse data from the \"{fileName}\" config file, but there was none.");
                return null;
            }

            var configValues = configFileData.Select(cfd =>
            {
                var keyAndValue = cfd.Split(':');
                if(keyAndValue.Length != 2)
                {
                    return new KeyValuePair<string, string>(string.Empty, string.Empty);
                }

                return new KeyValuePair<string, string>(keyAndValue.First(), keyAndValue.Last());
            });

            var configDictionary = new Dictionary<string, string>(configValues).Where(kvp => kvp.Key != default && kvp.Value != default);
            if(!configDictionary.Any())
            {
                GD.Print($"Unable to parse any data from the \"{fileName}\" config file.");
                return null;
            }

            return new Dictionary<string, string>(configDictionary);
        }

        private void UpdateColorsSystemProgramFile(Dictionary<string, Color> newColors)
        {
            var colorsExecutableFile = _directoryService.GetRootDirectory().FindDirectory("system/programs").FindFile("color");
            if (colorsExecutableFile == null)
            {
                GD.Print("Could not find the \"/system/programs/color\" executable file.");
                return;
            }

            var colorsContentsMinusReplacement = colorsExecutableFile.Contents.Split($"[COLORS{DirectoryConstants.HelpKeyValueSeparator}").First();
            var sortedColors = string.Join(", ", newColors.Keys.OrderBy(key => key).Select(key => key));
            colorsExecutableFile.Contents = string.Concat(colorsContentsMinusReplacement, DirectoryConstants.HelpLineSeparator, "[COLORS", DirectoryConstants.HelpKeyValueSeparator, sortedColors, "]");
        }

        private Dictionary<string, Color> GetLatestColorConfigValues()
        {
            if (ColorConfig == null)
            {
                GD.Print($"Attempted to parse color data from {ConfigFileConstants.ColorConfigFileName}, but \"{ConfigFileConstants.ColorConfigFileName}\" file was not found.");
                return new Dictionary<string, Color>()
                {
                    ["green"] = ColorConstants.TerminalGreen,
                    ["teal"] = ColorConstants.TerminalTeal
                };
            }

            var colors = ColorConfig.Select(config =>
            {
                if (!int.TryParse(config.Value, System.Globalization.NumberStyles.HexNumber, null, out int colorValidation))
                {
                    GD.Print($"Attempted to parse color data from {ConfigFileConstants.ColorConfigFileName}, but {config.Value} isn't a valid hex string.");
                    return default;
                }

                var colorValue = new Color($"#{config.Value}");
                return new KeyValuePair<string, Color>(config.Key, colorValue);
            });

            var allColors = new Dictionary<string, Color>(colors.Where(color => color.Key != default && color.Value != default));
            UpdateColorsSystemProgramFile(allColors);
            return allColors;
        }

        private int GetLatestUserConfigVolume() => UserConfig.GetLatestIntegerConfig(100, ConfigFileConstants.UserConfigFileName);

        private int GetLatestDisplayEffectConfig() => DisplayConfig.GetLatestIntegerConfig(100, ConfigFileConstants.DisplayConfigFileName);
    }
}
