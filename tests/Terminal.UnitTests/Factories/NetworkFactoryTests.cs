using System.Net;

using Terminal.Factories;

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
    }
}
