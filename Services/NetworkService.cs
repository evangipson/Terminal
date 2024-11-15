using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Extensions;
using Terminal.Models;

namespace Terminal.Services
{
    /// <summary>
    /// A global singleton that is responsible for getting networking information.
    /// </summary>
    public partial class NetworkService : Node
    {
        /// <summary>
        /// Invoked when showing networking information.
        /// <para>
        /// Will continue to run unless unsubscribed after running the method.
        /// </para>
        /// </summary>
        public event Action<string> OnShowNetwork;

        private static readonly Dictionary<string, List<string>> _networkCommandFlags = new()
        {
            ["active"] = new() { "-a", "--active" },
            ["name"] = new() { "-n", "--name" },
            ["device"] = new() { "-d", "--device" },
            ["ipv6"] = new() { "-v6", "--ipv6" },
            ["ipv8"] = new() { "-v8", "--ipv8" },
        };
        private const int nameColumnLength = -10;
        private const int deviceColumnLength = -8;
        private const int ipv6ColumnLength = -22;
        private const int ipv8ColumnLength = -18;

        private DirectoryService _directoryService;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
        }

        private Dictionary<string, List<string>> NetworkDevices
        {
            get
            {
                var networkDirectory = _directoryService.GetRootDirectory().FindDirectory("system").FindDirectory("network");
                var networkResponse = networkDirectory.Entities.Where(entity => !entity.IsDirectory).Select(entity =>
                {
                    var networkEntityList = entity.Contents.Split('\n');
                    return new KeyValuePair<string, List<string>>(entity.Name, networkEntityList.ToList());
                }).ToList();

                return new Dictionary<string, List<string>>(networkResponse);
            }
        }

        /// <summary>
        /// Shows networking information by reading the /system/network directory and files.
        /// </summary>
        /// <param name="arguments">
        /// The command line arguments.
        /// </param>
        public void ShowNetworkInformation(IEnumerable<string> arguments)
        {
            var showActive = arguments.Contains("-a");
            var showName = arguments.Contains("-n") || !arguments.Any() || showActive;
            var showDevice = arguments.Contains("-d") || !arguments.Any() || showActive;
            var showIpv6 = arguments.Contains("-ipv6") || arguments.Contains("-v6") || !arguments.Any();
            var showIpv8 = arguments.Contains("-ipv8") || arguments.Contains("-v8");

            // if there was an unexpected argument, tell the user
            List<string> unrecognizedArgs = new();
            foreach(var argument in arguments)
            {
                if(_networkCommandFlags.Values.All(flag => !flag.Contains(argument)))
                {
                    unrecognizedArgs.Add(argument);
                }
            }

            if(unrecognizedArgs.Any())
            {
                OnShowNetwork?.Invoke($"\"{unrecognizedArgs.First()}\" is an invalid argument for the \"network\" command. Use \"help network\" to see valid arguments.");
                return;
            }

            List<NetworkResponse> networkResponses = NetworkDevices.Select(networkDevice =>
            {
                var name = $"{networkDevice.Key}";
                var device = $"{networkDevice.Value.First(value => value.Contains("device:")).Replace("device:", string.Empty).Trim()}";
                var ipv6 = $"{networkDevice.Value.First(value => value.Contains("ipv6:")).Replace("ipv6:", string.Empty).Trim()}";
                var ipv8 = $"{networkDevice.Value.First(value => value.Contains("ipv8:")).Replace("ipv8:", string.Empty).Trim()}";
                var activeValue = $"{networkDevice.Value.First(value => value.Contains("active:")).Replace("active:", string.Empty).Trim()}";
                var active = activeValue?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
                return new NetworkResponse(name, device, ipv6, ipv8, active);
            }).ToList();

            List<string> columnTitles = new()
            {
                showName ? $"{"Name", nameColumnLength}" : string.Empty,
                showDevice ? $"{"Device", deviceColumnLength}" : string.Empty,
                showIpv6 ? $"{"Address (ipv6)", ipv6ColumnLength}" : string.Empty,
                showIpv8 ? $"{"Address (ipv8)", ipv8ColumnLength}" : string.Empty,
                showActive ? "Active" : string.Empty,
            };
            List<string> columnRowSeperators = new()
            {
                showName ? "═".Repeat(nameColumnLength * -1) : string.Empty,
                showDevice ? "═".Repeat(deviceColumnLength * -1) : string.Empty,
                showIpv6 ? "═".Repeat(ipv6ColumnLength * -1) : string.Empty,
                showIpv8 ? "═".Repeat(ipv8ColumnLength * -1) : string.Empty,
                showActive ? "═".Repeat(6) : string.Empty,
            };

            List<string> output = new()
            {
                string.Join("═══", columnRowSeperators.Where(columnRow => !string.IsNullOrEmpty(columnRow))),
                string.Join("   ", columnTitles.Where(columnTitle => !string.IsNullOrEmpty(columnTitle))),
                string.Join("═╤═", columnRowSeperators.Where(columnRow => !string.IsNullOrEmpty(columnRow)))
            };

            foreach(var networkResponse in networkResponses)
            {
                List<string> dataRow = new()
                {
                    showName ? $"{networkResponse.Name, nameColumnLength}" : string.Empty,
                    showDevice ? $"{networkResponse.Device, deviceColumnLength}" : string.Empty,
                    showIpv6 ? $"{networkResponse.Ipv6Address, ipv6ColumnLength}" : string.Empty,
                    showIpv8 ? $"{networkResponse.Ipv8Address, ipv8ColumnLength}" : string.Empty,
                    showActive ? $"{networkResponse.IsActive}".ToLowerInvariant() : string.Empty,
                };

                output.Add(string.Join(" │ ", dataRow.Where(row => !string.IsNullOrEmpty(row))));
            }
            output.Add(string.Join("═╧═", columnRowSeperators.Where(columnRow => !string.IsNullOrEmpty(columnRow))));

            List<string> wrappedOutput = new() { $"╔═{output.First(line => !string.IsNullOrEmpty(line))}═╗" };
            for(var line = 1; line < output.Count - 1; line++)
            {
                if(line == 2)
                {
                    wrappedOutput.Add($"╠═{output.ElementAt(line)}═╣");
                    continue;
                }
                wrappedOutput.Add($"║ {output.ElementAt(line)} ║");
            }
            wrappedOutput.Add($"╚═{output.Last(line => !string.IsNullOrEmpty(line))}═╝");

            OnShowNetwork?.Invoke(string.Join("\n", wrappedOutput));
        }
    }
}