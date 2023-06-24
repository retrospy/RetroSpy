#if OS_WINDOWS

using System.Collections.ObjectModel;

namespace GBPUpdaterX2
{
    public class SerialPortDescription // custom struct with our desired values
    {
        public string? PortName { get; internal set; }

        public string? VID { get; internal set; }

        public string? PID { get; internal set; }

        public string? Description { get; internal set; }

        public int iVID { get; internal set; }

        public int iPID { get; internal set; }

        public bool IsCH340 { get; internal set; }

        public string? USBDeviceName { get; internal set; }

        public bool IsFakeCH340 { get; internal set; }

        public ReadOnlyCollection<string>? HardwareId { get; internal set; }

        public string? DriverVersion { get; internal set; }

        public string? DriverInf { get; internal set; }

        public bool Matches(int vid, int pid)
        {
            return vid == iVID && pid == iPID;
        }
    }
}

#endif