#if OS_WINDOWS

using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace GBPUpdaterX2
{
    internal static partial class OEMDriversHelper
    {
        public static IReadOnlyList<OEMDriverInf> LocateDrivers(TextBox txtBox, string hwid = "USB\\VID_1A86&PID_5523")
        {
            var res = new List<OEMDriverInf>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var key = Registry.LocalMachine.OpenSubKey("SYSTEM\\DriverDatabase\\DeviceIds\\" + hwid);
                if (key != null)
                {
                    foreach (var fileName in key.GetValueNames())
                    {
                        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "inf", fileName);
                        if (File.Exists(filePath))
                        {
                            try
                            {
                                res.Add(new OEMDriverInf(filePath, GetDriverVersion(filePath) ?? string.Empty));
                            }
                            catch
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    txtBox.Text += $"Could not parse driver {filePath}, file ignored\n";
                                    txtBox.CaretIndex = int.MaxValue;
                                });
                            }
                        }
                    }
                }
            }
            return res.AsReadOnly();
        }

        private static string? GetDriverVersion(string? fileName)
        {
            if (fileName != null)
            {
                var text = File.ReadAllText(fileName);
                var match = DriverVersionRegEx().Match(text);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
            return null;
        }

        public static bool UninstallDriverVersion(string deviceId, string driverVersion, TextBox txtBox)
        {
            Dispatcher.UIThread.Post(() =>
            {
                txtBox.Text += $"Searching installed drivers for {deviceId}...\n";
                txtBox.CaretIndex = int.MaxValue;
            });

            var knownDrivers = LocateDrivers(txtBox, deviceId);

            if (!knownDrivers.Any())
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtBox.Text += $"No driver found\n";
                    txtBox.CaretIndex = int.MaxValue;
                });

                return false;
            }

            foreach (var driver in knownDrivers)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtBox.Text += $"Found driver : {driver.FileName}, {driver.DriverVer}\n";
                    txtBox.CaretIndex = int.MaxValue;
                });
            }

            var guiltyDriver = knownDrivers.FirstOrDefault(i => i.DriverVer.EndsWith(driverVersion));
            if (guiltyDriver != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtBox.Text += $"Driver non compatible with fake CH340 found : {guiltyDriver.FileName}, {guiltyDriver.DriverVer}\n";
                    txtBox.CaretIndex = int.MaxValue;
                });

                PNPUtilHelper.UninstallDriver(guiltyDriver, txtBox);
                return true;
            }

            return false;
        }

        [GeneratedRegex("[ \\t]*DriverVer.*=[ \\t]*(.*)[ \\t]*$", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-US")]
        private static partial Regex DriverVersionRegEx();
    }
}

#endif