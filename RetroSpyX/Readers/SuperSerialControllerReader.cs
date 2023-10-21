using System;

namespace RetroSpy.Readers
{
    public sealed class SuperSerialControllerReader : IControllerReader, IDisposable
    {
        public event EventHandler<ControllerStateEventArgs>? ControllerStateChanged;

        public event EventHandler? ControllerDisconnected;

        private readonly Func<byte[]?, ControllerStateEventArgs?> _packetParser;
        private SuperSerialMonitor? _serialMonitor;

        public SuperSerialControllerReader(string? portName, bool useLagFix, bool isFullSpeed, Func<byte[]?, ControllerStateEventArgs?> packetParser)
        {
            _packetParser = packetParser;

            _serialMonitor = new SuperSerialMonitor(portName, useLagFix, isFullSpeed);
            _serialMonitor.PacketReceived += SuperSerialMonitor_PacketReceived;
            _serialMonitor.Disconnected += SerialMonitor_Disconnected;
            _serialMonitor.Start();
        }

        private void SerialMonitor_Disconnected(object? sender, EventArgs e)
        {
            Finish();
            ControllerDisconnected?.Invoke(this, EventArgs.Empty);
        }

        private void SuperSerialMonitor_PacketReceived(object? sender, SuperPacketDataEventArgs packet)
        {
            if (ControllerStateChanged != null)
            {
                ControllerStateEventArgs? state = _packetParser(packet.GetPacket());
                if (state != null)
                {
                    ControllerStateChanged(this, state);
                }
            }
        }

        public void Finish()
        {
            if (_serialMonitor != null)
            {
                _serialMonitor.Stop();
                _serialMonitor.Dispose();
                _serialMonitor = null;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Finish();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}