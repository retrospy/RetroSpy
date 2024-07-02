using System;

namespace RetroSpy
{
    [Serializable]
    public class SSHMonitorDisconnectException : Exception
    {
        public SSHMonitorDisconnectException(string message) : base(message)
        {
        }

        public SSHMonitorDisconnectException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public SSHMonitorDisconnectException()
        {
        }
    }
}
