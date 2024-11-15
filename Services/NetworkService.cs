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
            ["noname"] = new() { "-nn", "--no-name" },
            ["nodevice"] = new() { "-nd", "--no-device" },
            ["noaddress"] = new() { "-na", "--no-address" },
            ["ipv8"] = new() { "-v8", "--ipv8" },
        };
        private const int nameColumnLength = -10;
        private const int deviceColumnLength = -8;
        private const int ipv6ColumnLength = -39;
        private const int ipv8ColumnLength = -16;
        private const int activeColumnLength = -6;

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
            var hideAddresses = _networkCommandFlags["noaddress"].Any(flag => arguments.Contains(flag));
            var showName = _networkCommandFlags["noname"].All(flag => !arguments.Contains(flag));
            var showDevice = _networkCommandFlags["nodevice"].All(flag => !arguments.Contains(flag));
            var showActive = _networkCommandFlags["active"].Any(flag => arguments.Contains(flag));
            var showIpv8 = _networkCommandFlags["ipv8"].Any(flag => arguments.Contains(flag)) && !hideAddresses;
            var showIpv6 = !hideAddresses && !showIpv8;

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
                showActive ? $"{"Active", activeColumnLength}" : string.Empty,
            };
            List<string> columnRowSeperators = new()
            {
                showName ? "═".Repeat(nameColumnLength * -1) : string.Empty,
                showDevice ? "═".Repeat(deviceColumnLength * -1) : string.Empty,
                showIpv6 ? "═".Repeat(ipv6ColumnLength * -1) : string.Empty,
                showIpv8 ? "═".Repeat(ipv8ColumnLength * -1) : string.Empty,
                showActive ? "═".Repeat(activeColumnLength * -1) : string.Empty,
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
                    showActive ? $"{networkResponse.IsActive.ToString().ToLowerInvariant(), activeColumnLength}" : string.Empty,
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

        public void ShowPingResponse(string address)
        {
        }
    }
}