using Godot;

namespace Terminal.Models
{
    /// <summary>
    /// Represents a "uniform" (exposed) variable for a shader from the shader code.
    /// </summary>
    public class ShaderParameter
    {
        /// <summary>
        /// The case-sensitive name of a <see cref="ShaderParameter"/>, as it appears in the shader code.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The current value of a <see cref="ShaderParameter"/>.
        /// </summary>
        public Variant Value { get; set; }

        /// <summary>
        /// The minimum value of a <see cref="ShaderParameter"/>.
        /// <para>
        /// Defaults to <c>0.0</c>.
        /// </para>
        /// </summary>
        public Variant MinValue { get; set; } = 0.0;

        /// <summary>
        /// The maximum value of a <see cref="ShaderParameter"/>.
        /// </summary>
        public Variant MaxValue { get; set; }
    }
}
