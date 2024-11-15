using Terminal.Factories;

namespace Terminal.Models
{
    public readonly struct IpAddressV8
    {
        public IpAddressV8()
        {
            Address = NetworkFactory.GetNewIpAddressV8();
        }

        public readonly string Address { get; }
    }
}
