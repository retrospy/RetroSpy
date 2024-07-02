using System;
using System.Globalization;

namespace RetroSpy
{
    [Serializable]
    public class ConfigParseException : Exception
    {
        public ConfigParseException()
            : base() { }

        public ConfigParseException(string message)
            : base(message) { }

        public ConfigParseException(string format, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, format, args)) { }

        public ConfigParseException(string message, Exception innerException)
            : base(message, innerException) { }

        public ConfigParseException(string format, Exception innerException, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, format, args), innerException) { }
    }
}