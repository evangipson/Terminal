using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        /// <summary>
        /// Invoked when the ping command is run.
        /// <para>
        /// Will continue to run unless unsubscribed after running the method.
        /// </para>
        /// </summary>
        public event Action<string> OnPing;

        /// <summary>
        /// Invoked when the ping command is done.
        /// <para>
        /// Will continue to run unless unsubscribed after running the method.
        /// </para>
        /// </summary>
        public event Action<string> OnPingDone;

        private static readonly Dictionary<string, List<string>> _networkCommandFlags = new()
        {
            ["active"] = new() { "-a", "--active" },
            ["noname"] = new() { "-nn", "--no-name" },
            ["nodevice"] = new() { "-nd", "--no-device" },
            ["noaddress"] = new() { "-na", "--no-address" },
            ["ipv8"] = new() { "-v8", "--ipv8" },
        };
        private static readonly Dictionary<string, List<string>> _pingCommandFlags = new()
        {
            ["ipv8"] = new() { "-v8", "--ipv8" },
            ["ipv6"] = new() { "-v6", "--ipv6" },
        };
        private const int nameColumnLength = -10;
        private const int deviceColumnLength = -8;
        private const int ipv6ColumnLength = -39;
        private const int ipv8ColumnLength = -16;
        private const int activeColumnLength = -6;

        private DirectoryService _directoryService;
        private TimerService _timerService;
        private Random _random;
        private int _pingsResponded = 0;
        private int _pingsSent = 0;
        private string _pingAddress = string.Empty;
        private double _totalPingMilliseconds = 0;
        private double _nextPingMilliseconds = 0;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            _random = new(DateTime.UtcNow.GetHashCode());
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
            foreach (var argument in arguments)
            {
                if (_networkCommandFlags.Values.All(flag => !flag.Contains(argument)))
                {
                    unrecognizedArgs.Add(argument);
                }
            }

            if (unrecognizedArgs.Any())
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

            foreach (var networkResponse in networkResponses)
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
            for (var line = 1; line < output.Count - 1; line++)
            {
                if (line == 2)
                {
                    wrappedOutput.Add($"╠═{output.ElementAt(line)}═╣");
                    continue;
                }
                wrappedOutput.Add($"║ {output.ElementAt(line)} ║");
            }
            wrappedOutput.Add($"╚═{output.Last(line => !string.IsNullOrEmpty(line))}═╝");

            OnShowNetwork?.Invoke(string.Join("\n", wrappedOutput));
        }

        public void StartPingResponse(string address, IEnumerable<string> arguments)
        {
            var pingOnlyIpv6 = _pingCommandFlags["ipv6"].Any(flag => arguments.Contains(flag));
            var pingOnlyIpv8 = _pingCommandFlags["ipv8"].Any(flag => arguments.Contains(flag));

            // if there was an unexpected argument, tell the user
            List<string> unrecognizedArgs = new();
            foreach (var argument in arguments)
            {
                if (_pingCommandFlags.Values.All(flag => !flag.Contains(argument)))
                {
                    unrecognizedArgs.Add(argument);
                }
            }

            if (unrecognizedArgs.Any())
            {
                OnPingDone?.Invoke($"\"{unrecognizedArgs.First()}\" is an invalid argument for the \"ping\" command. Use \"help ping\" to see valid arguments.");
                return;
            }

            var pingAddress = string.Empty;
            if (!pingOnlyIpv8 && IPAddress.TryParse(address, out IPAddress ipAddress))
            {
                pingAddress = ipAddress.MapToIPv6().ToString();
            }

            if (!pingOnlyIpv6 && IpAddressV8.TryParse(address, out IpAddressV8 ipv8Address))
            {
                pingAddress = ipv8Address.Address;
            }

            if (string.IsNullOrEmpty(pingAddress))
            {
                var failureMessage = $"Could not ping {address}, it was not a valid ipv6 or ipv8 address.";
                if(pingOnlyIpv6 && !pingOnlyIpv8)
                {
                    failureMessage = $"Could not ping {address}, it was not a valid ipv6 address.";
                }
                if(pingOnlyIpv8 && !pingOnlyIpv6)
                {
                    failureMessage = $"Could not ping {address}, it was not a valid ipv8 address.";
                }
                OnPingDone?.Invoke(failureMessage);
                return;
            }

            ResetPing();
            _pingAddress = pingAddress;
            _nextPingMilliseconds = _random.Next(250, 1500) * (1.0 + _random.NextDouble());
            _totalPingMilliseconds += _nextPingMilliseconds;
            _pingsSent++;
            _timerService = new((_, _) => Ping(), _nextPingMilliseconds);
        }

        public void Ping()
        {
            if(_pingsResponded > 4)
            {
                CallDeferred("FinishPing");
                return;
            }

            _timerService.Done();
            CallDeferred("DeferredPing");
        }

        /// <summary>
        /// Interrupts a ping in progress, if one is happening. Does nothing otherwise.
        /// </summary>
        public void InterruptPing()
        {
            if(_pingsResponded == 0 && _pingsSent == 0 && _totalPingMilliseconds == 0 && _pingAddress == string.Empty)
            {
                return;
            }

            FinishPing();
        }

        private void FinishPing()
        {
            _timerService.Done();
            OnPingDone?.Invoke($"--- {_pingAddress} ping statistics ---\n{_pingsSent} packets transmitted, {_pingsResponded} received, 0% packet loss, time {Math.Round(_totalPingMilliseconds, 2)}ms");

            ResetPing();
        }

        private void DeferredPing()
        {
            _pingsResponded++;
            OnPing?.Invoke($"64 bytes from {_pingAddress}: ttl=55 time={Math.Round(_nextPingMilliseconds, 2)}ms");

            _nextPingMilliseconds = _random.Next(250, 1500) * (1.0 + _random.NextDouble());
            _totalPingMilliseconds += _nextPingMilliseconds;

            _timerService.Wait(_nextPingMilliseconds);
            _timerService.Resume();
            if (_pingsSent < 5)
            {
                _pingsSent++;
            }
        }

        private void ResetPing()
        {
            _pingsResponded = 0;
            _pingsSent = 0;
            _totalPingMilliseconds = 0;
            _nextPingMilliseconds = 0;
            _pingAddress = string.Empty;
        }
    }
}