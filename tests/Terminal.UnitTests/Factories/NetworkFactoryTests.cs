using System.Net;

using Terminal.Factories;
using Terminal.Models;

namespace Terminal.UnitTests.Factories
{
    public class NetworkFactoryTests
    {
        [Fact]
        public void CreateIpAddressV6_ShouldCreateValidIpv6Address_WithoutLoopback()
        {
            var result = NetworkFactory.CreateIpAddressV6();

            Assert.True(IPAddress.TryParse(result, out IPAddress? _));
        }

        [Fact]
        public void CreateIpAddressV6_ShouldCreateValidLoopbackIpv6Address_WithLoopback()
        {
            var result = NetworkFactory.CreateIpAddressV6(true);

            Assert.True(IPAddress.TryParse(result, out IPAddress? resultAsAddress));
            Assert.True(resultAsAddress.IsIPv6LinkLocal);
        }

        [Fact]
        public void CreateIpAddressV8_ShouldCreateValidIpv8Address_WithoutLoopback()
        {
            var result = NetworkFactory.CreateIpAddressV8();

            Assert.True(IpAddressV8.TryParse(result, out IpAddressV8 _));
        }

        [Fact]
        public void CreateIpAddressV8_ShouldCreateValidLoopbackIpv8Address_WithLoopback()
        {
            var result = NetworkFactory.CreateIpAddressV8(true);

            Assert.True(IpAddressV8.TryParse(result, out IpAddressV8 resultAsAddress));
            Assert.StartsWith("loop", resultAsAddress.Address.ToLower());
        }
    }
}
