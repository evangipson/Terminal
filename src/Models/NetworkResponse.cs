namespace Terminal.Models
{
    /// <summary>
    /// Represents a response containing network information.
    /// </summary>
    /// <remarks>
    /// Creates a new fully-qualified <see cref="NetworkResponse"/>.
    /// </remarks>
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
    public readonly struct NetworkResponse(string name, string device, string ipv6, string ipv8, bool isActive = false)
    {
        /// <summary>
        /// The name of the network.
        /// </summary>
        public readonly string Name { get; } = name;

        /// <summary>
        /// The networking device.
        /// </summary>
        public readonly string Device { get; } = device;

        /// <summary>
        /// The ipv6 address of the network.
        /// </summary>
        public readonly string Ipv6Address { get; } = ipv6;

        /// <summary>
        /// The ipv8 address of the network.
        /// </summary>
        public readonly string Ipv8Address { get; } = ipv8;

        /// <summary>
        /// A flag indicating if the network is active.
        /// </summary>
        public readonly bool IsActive { get; } = isActive;
    }
}
