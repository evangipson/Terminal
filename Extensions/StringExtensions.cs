using System.Text;

namespace Terminal.Extensions
{
    /// <summary>
    /// A <see langword="static"/> collection of extension methods for <see langword="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Repeats a <see langword="string"/> <paramref name="count"/> amount of times.
        /// </summary>
        /// <param name="input">The <see langword="string"/> to repeat.</param>
        /// <param name="count">The amount of times to repeat the <paramref name="input"/>.</param>
        /// <returns>
        /// A <see langword="string"/> <paramref name="count"/> amount of times. Defaults to
        /// returning the <paramref name="input"/> if it's <see langword="null"/> or empty, or
        /// if <paramref name="count"/> is less than or equal to one.
        /// </returns>
        public static string Repeat(this string input, int count)
        {
            if(string.IsNullOrEmpty(input) || count <= 1)
            {
                return input;
            }

            var builder = new StringBuilder(input.Length * count);
            for(int i = 0; i < count; i++)
            {
                builder.Append(input);
            }

            return builder.ToString();
        }
    }
}
