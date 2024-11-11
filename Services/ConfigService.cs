using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Extensions;

namespace Terminal.Services
{
    public partial class ConfigService : Node
    {
        private DirectoryService _directoryService;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
        }

        public Dictionary<string, string> ColorConfig => LoadConfigFile("color.conf");

        public Dictionary<string, string> UserConfig => LoadConfigFile("user.conf", true);

        public Dictionary<string, Color> Colors
        {
            get
            {
                var colors = ColorConfig.Select(config =>
                {
                    if (!int.TryParse(config.Value, System.Globalization.NumberStyles.HexNumber, null, out int colorValidation))
                    {
                        GD.Print($"Attempted to parse color data from color.conf, but {config.Value} isn't a valid hex string.");
                        return default;
                    }

                    var colorValue = new Color($"#{config.Value}");
                    return new KeyValuePair<string, Color>(config.Key, colorValue);
                });

                var allColors = new Dictionary<string, Color>(colors.Where(color => color.Key != default && color.Value != default));
                UpdateColorsSystemProgramFile(allColors);
                return allColors;
            }
        }

        public Dictionary<string, Color> ExecutableColors => new(Colors.Select(color =>
        {
            var invertedColor = Colors.SkipWhile(c => c.Key != color.Key).Skip(1).FirstOrDefault();
            return new KeyValuePair<string, Color>(color.Key, invertedColor.Key == default ? Colors.First().Value : invertedColor.Value);
        }));

        private Dictionary<string, string> LoadConfigFile(string fileName, bool userConfig = false)
        {
            var configFile = userConfig ? _directoryService.GetRelativeFile(fileName) : _directoryService.GetAbsoluteFile(fileName);
            if (configFile == null)
            {
                GD.Print($"Attempted to load the \"{fileName}\" config file, but it was not found.");
                return null;
            }

            var configFileData = configFile.Contents.Split('\n');
            if (configFileData.Length == 0)
            {
                GD.Print($"Attempted to parse data from the \"{fileName}\" config file, but there was none.");
                return null;
            }

            var configValues = configFileData.Select(cfd =>
            {
                var keyAndValue = cfd.Split(':');
                if(keyAndValue.Length != 2)
                {
                    return default;
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
    }
}
