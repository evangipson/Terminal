using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Terminal.Factories
{
    
    public static class NetworkFactory
    {
        private static readonly Random _random = new(DateTime.UtcNow.GetHashCode());
        private const string _addressChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-+_=:'\"{}\\/|()!@#$%;^*&";

        public static string GetNewIpAddressV6(bool loopback = false) => new IPAddress(GetNewIpAddressV6Bytes(loopback))
            .MapToIPv6()
            .ToString();

        public static byte[] GetNewIpAddressV6Bytes(bool loopback = false)
        {
            if (!loopback)
            {
                byte[] bytes = new byte[16];
                _random.NextBytes(bytes);

                return bytes;
            }

            List<byte> loopbackBytes = new(){ 0xFE, 0x80, (byte)_random.Next(), (byte)_random.Next() };
            byte[] otherBytes = new byte[12];
            _random.NextBytes(otherBytes);

            return loopbackBytes.Concat(otherBytes).ToArray();
        }

        public static string GetNewIpAddressV8(bool loopback = false)
        {
            if(!loopback)
            {
                return string.Concat(Enumerable.Range(0, 16).Select(x => _addressChars[_random.Next(_addressChars.Length)]));
            }

            return string.Concat("loop", string.Concat(Enumerable.Range(0, 12).Select(x => _addressChars[_random.Next(_addressChars.Length)])));
        }
    }
}
