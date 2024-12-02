using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Terminal.Models;

namespace Terminal.Factories
{
    /// <summary>
    /// A <see langword="static"/> factory that is responsible for creating network entities.
    /// </summary>
    public static class NetworkFactory
    {
        private static readonly Random _random = new(DateTime.UtcNow.GetHashCode());
        private const string _addressChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-+_=:'\"{}\\/|()!@#$%;^*&";

        /// <summary>
        /// Gets a new ipv6 address.
        /// </summary>
        /// <param name="loopback">
        /// A flag to determine if the ipv6 address will be for a loopback adapter.
        /// </param>
        /// <returns>
        /// A new ipv6 address.
        /// </returns>
        public static string CreateIpAddressV6(bool loopback = false) => new IPAddress(CreateIpAddressV6Bytes(loopback))
            .MapToIPv6()
            .ToString();

        /// <summary>
        /// Gets the <see langword="string"/> representation of a new <see cref="IpAddressV8"/>.
        /// </summary>
        /// <param name="loopback">
        /// A flag to determine if the <see cref="IpAddressV8"/> will be for a loopback adapter.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> representation of a new <see cref="IpAddressV8"/>.
        /// </returns>
        public static string CreateIpAddressV8(bool loopback = false)
        {
            if (!loopback)
            {
                return string.Concat(Enumerable.Range(0, 16).Select(x => _addressChars[_random.Next(_addressChars.Length)]));
            }

            return string.Concat("loop", string.Concat(Enumerable.Range(0, 12).Select(x => _addressChars[_random.Next(_addressChars.Length)])));
        }

        private static byte[] CreateIpAddressV6Bytes(bool loopback = false)
        {
            if (!loopback)
            {
                byte[] bytes = new byte[16];
                _random.NextBytes(bytes);

                return bytes;
            }

            List<byte> loopbackBytes = [0xFE, 0x80, (byte)_random.Next(), (byte)_random.Next()];
            byte[] otherBytes = new byte[12];
            _random.NextBytes(otherBytes);

            return [.. loopbackBytes, .. otherBytes];
        }
    }
}
