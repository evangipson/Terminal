using System;
using System.Linq;
using System.Text;
using Godot;

using Terminal.Models;

namespace Terminal.Extensions
{
    /// <summary>
    /// A <see langword="static"/> collection of extension methods for networking.
    /// </summary>
    public static class NetworkExtensions
    {
        /// <summary>
        /// Tries to parse the provided <paramref name="input"/> into an <see cref="IpAddressV8"/>.
        /// <para>
        /// Will return <see langword="default"/> if <paramref name="input"/> can't be parsed.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of <paramref name="input"/> to parse.
        /// </typeparam>
        /// <param name="input">
        /// The value to parse into an <see cref="IpAddressV8"/>.
        /// </param>
        /// <param name="address">
        /// The parsed <paramref name="input"/>, if it was successful, otherwise will be <see cref="default"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the parse succeeded, <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryParseIpAddressV8<T>(T input, out IpAddressV8 address)
        {
            try
            {
                byte[] inputBytes = ASCIIEncoding.ASCII.GetBytes(input.ToString());
                address = new(string.Concat(Convert.ToBase64String(inputBytes).Take(16)));

                return true;
            }
            catch(Exception exception)
            {
                address = default;
                GD.PrintErr(exception.Message, "Unable to parse provided input as ipv8 address.");

                return false;
            }
        }
    }
}
