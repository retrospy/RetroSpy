using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace GBPUpdaterX2
{
    public class SerialPortDescriptionHelper
    {
        private const string vidPattern = @"VID_([0-9A-F]{4})";
        private const string pidPattern = @"PID_([0-9A-F]{4})";
        private static string? LastComList = null;
        private static List<SerialPortDescription>? lastres;
        private const int CH340VID = 0x1A86;
        private const int CH340PID = 0x7523;

        private static object lockObj = new object();

        public static List<SerialPortDescription>? GetSerialPorts()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var pnames = SerialPort.GetPortNames().OrderBy(i => i).ToArray();
                var newComList = string.Join(",", pnames);
                if (LastComList != newComList || pnames.Length != (lastres?.Count ?? 0))
                {
                    lock (lockObj)
                    {
                        newComList = string.Join(",", SerialPort.GetPortNames().OrderBy(i => i));
                        if (LastComList != newComList || pnames.Length != lastres?.Count)
                        {
                            List<SerialPortDescription> res = new List<SerialPortDescription>();
                            try
                            {
                                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT DeviceID,PNPDeviceID,Caption,HardwareID FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\""))
                                {
                                    var ports = searcher.Get().OfType<ManagementObject>().ToList();
                                    foreach (var p in ports)
                                    {
                                        using (p)
                                        {
                                            try
                                            {
                                                SerialPortDescription c = new SerialPortDescription();
                                                var portNameMatches = Regex.Matches(p.GetPropertyValue("Caption").ToString() ?? string.Empty, "\\((COM[0-9]*)\\)");
                                                if (portNameMatches.Count == 0)
                                                {
                                                    continue;
                                                }

                                                c.PortName = portNameMatches.Cast<Match>().Last().Groups[1].Value;
                                                c.VID = p.GetPropertyValue("PNPDeviceID").ToString();
                                                var desc = p.GetPropertyValue("Caption").ToString();
                                                c.Description = Regex.Replace(desc ?? string.Empty, "\\(COM[0-9]*\\)", "").Trim();
                                                c.HardwareId = (p.GetPropertyValue("HardwareID") as string[] ?? new string[0]).ToList().AsReadOnly();

                                                Match mVID = Regex.Match(c.VID ?? string.Empty, vidPattern, RegexOptions.IgnoreCase);
                                                Match mPID = Regex.Match(c.VID ?? string.Empty, pidPattern, RegexOptions.IgnoreCase);

                                                if (mVID.Success)
                                                {
                                                    c.VID = mVID.Groups[1].Value;
                                                }
                                                else if (string.IsNullOrWhiteSpace(c.VID))
                                                {
                                                    continue;
                                                }

                                                if (mPID.Success)
                                                {
                                                    c.PID = mPID.Groups[1].Value;
                                                }
                                                else
                                                {
                                                    continue;
                                                }

                                                c.iVID = int.Parse(c.VID, System.Globalization.NumberStyles.HexNumber);
                                                c.iPID = int.Parse(c.PID, System.Globalization.NumberStyles.HexNumber);

                                                var ps = (c.VID ?? "") + "_" + (c.PID ?? "");

                                                c.USBDeviceName = GetUSBDeviceName(p);
                                                c.DriverVersion = GetUSBDeviceDriverVersion(p);
                                                c.DriverInf = GetUSBDeviceDriverInf(p);

                                                if (c.Matches(CH340VID, CH340PID))
                                                {
                                                    c.IsCH340 = true;
                                                    c.IsFakeCH340 = IsFakeCH340(c.USBDeviceName ?? string.Empty);
                                                }

                                                res.Add(c);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }

                                    foreach (var p in pnames)
                                    {
                                        if (!res.Any(i => i.PortName == p))
                                        {
                                            res.Add(new SerialPortDescription { PortName = p, VID = "0", PID = "0", iPID = 0, iVID = 0 });
                                        }
                                    }
                                }
                            }
                            catch (Exception )
                            {
                                foreach (var sp in SerialPort.GetPortNames())
                                {
                                    res.Add(new SerialPortDescription
                                    {
                                        Description = sp,
                                        PortName = sp,
                                    });
                                }
                            }
                            lastres = res;
                            LastComList = newComList;
                        }
                    }
                }
            }
            return lastres;
        }

        public static string? GetUSBDeviceName(ManagementObject mo) => GetStringDeviceProperty(mo, "DEVPKEY_Device_BusReportedDeviceDesc");

        public static string? GetUSBDeviceDriverVersion(ManagementObject mo) => GetStringDeviceProperty(mo, "DEVPKEY_Device_DriverVersion");

        public static string? GetUSBDeviceDriverInf(ManagementObject mo) => GetStringDeviceProperty(mo, "DEVPKEY_Device_DriverInfPath");

        public static string? GetStringDeviceProperty(ManagementObject mo, string key)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                var args = new object[] { new string[] { key }, null };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                mo.InvokeMethod("GetDeviceProperties", args);

                var mbos = (ManagementBaseObject[])args[1];
                if (mbos.Length > 0)
                {
                    // get value of property named "Data"
                    // not all objects have that so we enum all props here
#pragma warning disable CA1416 // Validate platform compatibility
                    var data = mbos[0].Properties.OfType<PropertyData>().FirstOrDefault(p => p.Name == "Data");
#pragma warning restore CA1416 // Validate platform compatibility
                    if (data != null)
                    {
                        return data.Value as string;
                    }
                }
            }
            return null;
        }

        private static bool IsFakeCH340(string USBDeviceName)
        {
            return USBDeviceName != "USB2.0-Serial";
        }

        public static IEnumerable<SerialPortDescription>? GetSerialPorts(int vid, int pid)
        {
            return GetSerialPorts()?.Where(i => i.iPID == pid && i.iVID == vid).ToList();
        }

        public static SerialPortDescription? GetSerialPort(int vid, int pid)
        {
            return GetSerialPorts()?.FirstOrDefault(i => i.iPID == pid && i.iVID == vid);
        }

        public static SerialPortDescription? GetSerialPort(string serialPort)
        {
            return GetSerialPorts()?.FirstOrDefault(i => i?.PortName?.Equals(serialPort, System.StringComparison.InvariantCultureIgnoreCase) == true);
        }
    }
}