namespace Terminal.Extensions
{
    /// <summary>
    /// A <see langword="static"/> collection of extension methods for manipulating numbers.
    /// </summary>
    public static class NumberExtensions
    {
        /// <summary>
        /// Converts the <paramref name="valueToConvert"/> from the original range (<paramref name="originalStart"/> - <paramref name="originalEnd"/>)
        /// to the new range (<paramref name="newStart"/> - <paramref name="newEnd"/>).
        /// </summary>
        /// <param name="valueToConvert">
        /// The value to convert.
        /// </param>
        /// <param name="originalStart">
        /// The start of the original range.
        /// </param>
        /// <param name="originalEnd">
        /// The end of the original range.
        /// </param>
        /// <param name="newStart">
        /// The start of the new range.
        /// </param>
        /// <param name="newEnd">
        /// The end of the new range.
        /// </param>
        /// <returns>
        /// The <paramref name="valueToConvert"/> converted to be within the new range (<paramref name="newStart"/> - <paramref name="newEnd"/>).
        /// </returns>
        public static int ConvertRange(this int valueToConvert, int originalStart, int originalEnd, int newStart, int newEnd)
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (int)(newStart + ((valueToConvert - originalStart) * scale));
        }

        /// <summary>
        /// Converts the <paramref name="valueToConvert"/> from the original range (<paramref name="originalStart"/> - <paramref name="originalEnd"/>)
        /// to the new range (<paramref name="newStart"/> - <paramref name="newEnd"/>).
        /// </summary>
        /// <param name="valueToConvert">
        /// The value to convert.
        /// </param>
        /// <param name="originalStart">
        /// The start of the original range.
        /// </param>
        /// <param name="originalEnd">
        /// The end of the original range.
        /// </param>
        /// <param name="newStart">
        /// The start of the new range.
        /// </param>
        /// <param name="newEnd">
        /// The end of the new range.
        /// </param>
        /// <returns>
        /// The <paramref name="valueToConvert"/> converted to be within the new range (<paramref name="newStart"/> - <paramref name="newEnd"/>).
        /// </returns>
        public static double ConvertRange(this int valueToConvert, int originalStart, int originalEnd, double newStart, double newEnd)
        {
            double scale = (newEnd - newStart) / (originalEnd - originalStart);
            return newStart + ((valueToConvert - originalStart) * scale);
        }
    }
}
