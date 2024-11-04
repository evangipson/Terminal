using Godot;
using System;
using System.Collections.Generic;

namespace Terminal.Constants
{
    public static class ColorConstants
    {
        public static readonly Dictionary<string, Color> TerminalColors = new(StringComparer.OrdinalIgnoreCase)
        {
            ["green"] = new("#377a1c"),
            ["blue"] = new("#1c387a"),
            ["red"] = new("#7a1c38"),
            ["orange"] = new("#7a2f1c"),
            ["purple"] = new("#5e1c7a"),
            ["teal"] = new("#1c677a")
        };

        public static readonly Color Black = new("#000000");
    }
}
