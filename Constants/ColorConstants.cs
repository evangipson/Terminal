using Godot;
using System;
using System.Collections.Generic;

using Terminal.Models;
using Terminal.Enums;

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
        /// The terminal's "blue" color: <c>#1c387a</c>.
        /// </summary>
        public static readonly Color TerminalBlue = new("#1c387a");

        /// <summary>
        /// The terminal's "red" color: <c>#7a1c38</c>.
        /// </summary>
        public static readonly Color TerminalRed = new("#7a1c38");

        /// <summary>
        /// The terminal's "orange" color: <c>#7a2f1c</c>.
        /// </summary>
        public static readonly Color TerminalOrange = new("#7a2f1c");

        /// <summary>
        /// The terminal's "purple" color: <c>#5e1c7a</c>.
        /// </summary>
        public static readonly Color TerminalPurple = new("#5e1c7a");

        /// <summary>
        /// The terminal's "teal" color: <c>#1c677a</c>.
        /// </summary>
        public static readonly Color TerminalTeal = new("#1c677a");

        /// <summary>
        /// The termina's "black" color: <c>#000000</c>.
        /// </summary>
        public static readonly Color TerminalBlack = new("#000000");

        /// <summary>
        /// A collection of all possible terminal colors.
        /// </summary>
        public static readonly Dictionary<string, Color> TerminalColors = new(StringComparer.OrdinalIgnoreCase)
        {
            ["green"] = TerminalGreen,
            ["blue"] = TerminalBlue,
            ["red"] = TerminalRed,
            ["orange"] = TerminalOrange,
            ["purple"] = TerminalPurple,
            ["teal"] = TerminalTeal
        };

        /// <summary>
        /// A mapping of colors that a <see cref="DirectoryEntity"/> with <see cref="Permission.UserExecutable"/> or
        /// <see cref="Permission.AdminExecutable"/> will be rendered as when the user runs the <see cref="UserCommand.ListDirectory"/> command.
        /// </summary>
        public static readonly Dictionary<string, Color> ColorToExecutableColorMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["green"] = TerminalTeal,
            ["blue"] = TerminalPurple,
            ["red"] = TerminalOrange,
            ["orange"] = TerminalRed,
            ["purple"] = TerminalBlue,
            ["teal"] = TerminalGreen
        };
    }
}
