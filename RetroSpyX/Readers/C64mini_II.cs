using System;
using System.Text;

namespace RetroSpy.Readers
{
    public static class C64mini_II
    {
        private const int PACKET_SIZE = 17;

        private static readonly string?[] BUTTONS = {
            null, null, null, null, "1", "2", "tl", "tr", "a", "b", "c", "menu", null, null, null, null
        };

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length < PACKET_SIZE)
            {
                return null;
            }

            byte[] binaryPacket = StringToByteArray(Encoding.UTF8.GetString(packet, 0, packet.Length).Trim());

            ControllerStateBuilder outState = new();

            outState.SetButton(BUTTONS[4], (binaryPacket[5] & 0x10) != 0);
            outState.SetButton(BUTTONS[5], (binaryPacket[5] & 0x20) != 0);
            outState.SetButton(BUTTONS[6], (binaryPacket[5] & 0x40) != 0);
            outState.SetButton(BUTTONS[7], (binaryPacket[5] & 0x80) != 0);

            outState.SetButton(BUTTONS[8], (binaryPacket[6] & 0x01) != 0);
            outState.SetButton(BUTTONS[9], (binaryPacket[6] & 0x02) != 0);
            outState.SetButton(BUTTONS[10], (binaryPacket[6] & 0x04) != 0);
            outState.SetButton(BUTTONS[11], (binaryPacket[6] & 0x08) != 0);

            outState.SetButton("left", binaryPacket[0] < 0x7f);
            outState.SetButton("right", binaryPacket[0] > 0x7f);
            outState.SetButton("up", binaryPacket[1] < 0x7f);
            outState.SetButton("down", binaryPacket[1] > 0x7f);

            float x = 0;
            float y = 0;

            if (binaryPacket[0] > 0x7f)
            {
                x = 1;
            }
            else if (binaryPacket[0] < 0x7f)
            {
                x = -1;
            }

            if (binaryPacket[1] > 0x7f)
            {
                y = -1;
            }
            else if (binaryPacket[1] < 0x7f)
            {
                y = 1;
            }

            if (y != 0 || x != 0)
            {
                // point on the unit circle at the same angle
                double radian = Math.Atan2(y, x);
                float x1 = (float)Math.Cos(radian);
                float y1 = (float)Math.Sin(radian);

                // Don't let magnitude exceed the unit circle
                if (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) > 1.0)
                {
                    x = x1;
                    y = y1;
                }
            }

            outState.SetAnalog("x", x, binaryPacket[0]);
            outState.SetAnalog("y", y, binaryPacket[1]);

            return outState.Build();
        }
    }
}