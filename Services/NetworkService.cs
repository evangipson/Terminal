using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Extensions;

namespace Terminal.Services
{
    public partial class NetworkService : Node
    {
        /// <summary>
        /// Invoked when showing networking information.
        /// <para>
        /// Will continue to run unless unsubscribed after running the method.
        /// </para>
        /// </summary>
        public event Action<string> OnShowNetwork;

        private DirectoryService _directoryService;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
        }

        /// <summary>
        /// Shows networking information by reading the /system/network directory and files.
        /// </summary>
        /// <param name="arguments">
        /// The command line arguments.
        /// </param>
        public void ShowNetworkInformation(List<string> arguments)
        {
            var networkDirectory = _directoryService.GetRootDirectory().FindDirectory("system").FindDirectory("network");
            var networkResponse = networkDirectory.Entities.Where(entity => !entity.IsDirectory).Select(entity =>
            {
                var networkEntityList = entity.Contents.Split('\n');
                return new KeyValuePair<string, List<string>>(entity.Name, networkEntityList.ToList());
            }).ToList();

            List<Tuple<string, string, string, string>> networkColumnsOutput = new()
            {
                new("Name", "Device", "Address (ipv6)", "Address (ipv8)"),
                new("----", "------", "--------------", "--------------")
            };
            networkColumnsOutput.AddRange(networkResponse.Select(nri =>
            {
                var name = $"{nri.Key}";
                var device = $"{nri.Value.First(value => value.Contains("device:")).Replace("device:", string.Empty).Trim()}";
                var ipv6 = $"{nri.Value.First(value => value.Contains("ipv6:")).Replace("ipv6:", string.Empty).Trim()}";
                var ipv8 = $"{nri.Value.First(value => value.Contains("ipv8:")).Replace("ipv8:", string.Empty).Trim()}";
                return new Tuple<string, string, string, string>(name, device, ipv6, ipv8);
            }).ToList());

            OnShowNetwork?.Invoke(string.Join("\n", networkColumnsOutput.Select(networkOutput => $"{networkOutput.Item1,-10}{networkOutput.Item2,-10}{networkOutput.Item3,-24}{networkOutput.Item4}")));
        }
    }
}