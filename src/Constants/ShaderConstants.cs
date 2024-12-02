using System.Collections.Generic;

using Terminal.Models;
using Terminal.Shaders;

namespace Terminal.Constants
{
    /// <summary>
    /// A <see langword="static"/> collection of values relevant to the <see cref="Monitor"/> shader.
    /// </summary>
    public static class ShaderConstants
    {
        /// <summary>
        /// A collection of <see cref="ShaderParameter"/> used by the <see cref="Monitor"/> shader.
        /// </summary>
        public static readonly List<ShaderParameter> ShaderParameters =
        [
            new()
            {
                Name = "grille_opacity",
                MaxValue = 0.088
            },
            new()
            {
                Name = "scanlines_opacity",
                MaxValue = 0.2
            },
            new()
            {
                Name = "noise_opacity",
                MaxValue = 0.084
            },
            new()
            {
                Name = "vignette_opacity",
                MaxValue = 0.4
            },
            new()
            {
                Name = "static_noise_intensity",
                MaxValue = 0.02
            },
            new()
            {
                Name = "static_noise_intensity",
                MaxValue = 0.02
            },
            new()
            {
                Name = "static_noise_intensity",
                MaxValue = 0.02
            },
            new()
            {
                Name = "vignette_intensity",
                MaxValue = 0.2
            },
            new()
            {
                Name = "distort_intensity",
                MaxValue = 0.018
            }
        ];
    }
}
