namespace Terminal.Models
{
    /// <summary>
    /// Represents a response containing network information.
    /// </summary>
    public readonly struct NetworkResponse
    {
        /// <summary>
        /// Creates a new fully-qualified <see cref="NetworkResponse"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the network to show.
        /// </param>
        /// <param name="device">
        /// The network device to show.
        /// </param>
        /// <param name="ipv6">
        /// The ipv6 address of the network to show.
        /// </param>
        /// <param name="ipv8">
        /// The ipv8 address of the network to show.
        /// </param>
        /// <param name="isActive">
        /// A flag indicating if the shown network is active.
        /// </param>
        public NetworkResponse(string name, string device, string ipv6, string ipv8, bool isActive = false)
        {
            Name = name;
            Device = device;
            Ipv6Address = ipv6;
            Ipv8Address = ipv8;
            IsActive = isActive;
        }

        /// <summary>
        /// The name of the network.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// The networking device.
        /// </summary>
        public readonly string Device { get; }

        /// <summary>
        /// The ipv6 address of the network.
        /// </summary>
        public readonly string Ipv6Address { get; }

        /// <summary>
        /// The ipv8 address of the network.
        /// </summary>
        public readonly string Ipv8Address { get; }

        /// <summary>
        /// A flag indicating if the network is active.
        /// </summary>
        public readonly bool IsActive { get; }
    }
}
