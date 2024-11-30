using Godot;

using Terminal.Constants;
using Terminal.Extensions;
using Terminal.Services;

namespace Terminal.Shaders
{
    /// <summary>
    /// A <see cref="ColorRect"/> <see cref="Node"/> managed in Godot that contains the overlay shader effects on each screen.
    /// </summary>
    public partial class Monitor : ColorRect
    {
        private ConfigService _configService;
        private ShaderMaterial _shaderMaterial;

        public override void _Ready()
        {
            _configService = GetNode<ConfigService>(ServicePathConstants.ConfigServicePath);
            _shaderMaterial = (ShaderMaterial)Material;

            // Load the initial config value for monitor shader effect.
            AdjustShaderFromDisplayConfig(_configService.MonitorShaderIntensity);

            // Whenever the user updates the config file, adjust it in real time.
            _configService.OnMonitorIntensityConfigChange += AdjustShaderFromDisplayConfig;
        }

        private void AdjustShaderFromDisplayConfig(int displayConfigValue)
        {
            foreach(var shaderParameter in ShaderConstants.ShaderParameters)
            {
                shaderParameter.Value = displayConfigValue.ConvertRange(0, 100, shaderParameter.MinValue.AsDouble(), shaderParameter.MaxValue.AsDouble());
                _shaderMaterial.SetShaderParameter(shaderParameter.Name, shaderParameter.Value);
            }
        }
    }
}
