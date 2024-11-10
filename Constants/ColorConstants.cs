using Godot;

namespace Terminal.Constants
{
    /// <summary>
    /// A <see langword="static"/> service that manages and stores general <see cref="Color"/> information.
    /// </summary>
    public static class ColorConstants
    {
        /// <summary>
        /// The terminal's "green" color: <c>#377a1c</c>.
        /// </summary>
        public static readonly Color TerminalGreen = new("#377a1c");

        /// <summary>
        /// The terminal's "teal" color: <c>#1c677a</c>.
        /// </summary>
        public static readonly Color TerminalTeal = new("#1c677a");

        /// <summary>
        /// The termina's "black" color: <c>#000000</c>.
        /// </summary>
        public static readonly Color TerminalBlack = new("#000000");
    }
}
