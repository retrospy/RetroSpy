using Avalonia.Threading;
using System;
using Vortice.XInput;

namespace RetroSpy.Readers
{
    public static class USBMouse
    {
        private const int PACKET_SIZE = 9;

        private static readonly string?[] BUTTONS = {
            "left", "right", "middle", null, null, null, null, null
        };

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length != PACKET_SIZE)
            {
                return null;
            }

            string str = System.Text.Encoding.Default.GetString(packet, 0, 8);

            sbyte[] binaryPacket = new sbyte[4];

            try
            {
                for (int i = 0; i < str.Length; i += 2)
                {
                    binaryPacket[i / 2] = (sbyte)Convert.ToByte(str.Substring(i, 2), 16);
                }
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }

            ControllerStateBuilder outState = new();

            for (int i = 0; i < 8; ++i)
            {
                if (BUTTONS[i] == null)
                    continue;

                outState.SetButton(BUTTONS[i], (binaryPacket[0] & (1 << i)) != 0);
            }

            outState.SetButton("scroll_down", (byte)binaryPacket[3] == 0xFF);
            outState.SetButton("scroll_up", binaryPacket[3] == 0x01);

            sbyte x = Math.Abs(binaryPacket[1]) > 10 ? binaryPacket[1] : (sbyte)0;
            sbyte y = Math.Abs(binaryPacket[2]) > 10 ? binaryPacket[2] : (sbyte)0; ;

            SignalTool.SetMouseProperties(x/128.0f, -y / 128.0f, x, y, outState);

            return outState.Build();
        }
    }
}