using System;
using System.Text;

namespace RetroSpy.Readers
{
    public static class GenesisMiniReader_II
    {
        private const int PACKET_SIZE = 17;

        private static readonly string?[] THREE_BUTTONS = {
            null, null, null, null, "y", "b", "a", "x", "z", "c", null, null, "mode", "start", null, null
        };

        private static readonly string?[] SIX_BUTTONS = {
            null, null, null, null, "x", "a", "b", "y", "c", "z", "l", "r", "mode", "start", null, null
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

            if ((binaryPacket[5] & 0x01) != 0 && (binaryPacket[5] & 0x02) != 0 && (binaryPacket[5] & 0x04) != 0 && (binaryPacket[5] & 0x08) != 0)
            {
                outState.SetButton(THREE_BUTTONS[4], (binaryPacket[5] & 0x10) != 0);
                outState.SetButton(THREE_BUTTONS[5], (binaryPacket[5] & 0x20) != 0);
                outState.SetButton(THREE_BUTTONS[6], (binaryPacket[5] & 0x40) != 0);
                outState.SetButton(THREE_BUTTONS[7], (binaryPacket[5] & 0x80) != 0);

                outState.SetButton(THREE_BUTTONS[8], (binaryPacket[6] & 0x01) != 0);
                outState.SetButton(THREE_BUTTONS[9], (binaryPacket[6] & 0x02) != 0);
                outState.SetButton(THREE_BUTTONS[12], (binaryPacket[6] & 0x10) != 0);
                outState.SetButton(THREE_BUTTONS[13], (binaryPacket[6] & 0x20) != 0);

                outState.SetButton("left", binaryPacket[3] < 0x7f);
                outState.SetButton("right", binaryPacket[3] > 0x7f);
                outState.SetButton("up", binaryPacket[4] < 0x7f);
                outState.SetButton("down", binaryPacket[4] > 0x7f);
            }
            else if ((binaryPacket[5] & 0x01) == 0 && (binaryPacket[5] & 0x02) == 0 && (binaryPacket[5] & 0x04) == 0 && (binaryPacket[5] & 0x08) == 0)
            {
                outState.SetButton(SIX_BUTTONS[4], (binaryPacket[5] & 0x10) != 0);
                outState.SetButton(SIX_BUTTONS[5], (binaryPacket[5] & 0x20) != 0);
                outState.SetButton(SIX_BUTTONS[6], (binaryPacket[5] & 0x40) != 0);
                outState.SetButton(SIX_BUTTONS[7], (binaryPacket[5] & 0x80) != 0);

                outState.SetButton(SIX_BUTTONS[8], (binaryPacket[6] & 0x01) != 0);
                outState.SetButton(SIX_BUTTONS[9], (binaryPacket[6] & 0x02) != 0);
                outState.SetButton(SIX_BUTTONS[10], (binaryPacket[6] & 0x04) != 0);
                outState.SetButton(SIX_BUTTONS[11], (binaryPacket[6] & 0x08) != 0);
                outState.SetButton(SIX_BUTTONS[12], (binaryPacket[6] & 0x10) != 0);
                outState.SetButton(SIX_BUTTONS[13], (binaryPacket[6] & 0x20) != 0);

                outState.SetButton("left", binaryPacket[3] < 0x80);
                outState.SetButton("right", binaryPacket[3] > 0x80);
                outState.SetButton("up", binaryPacket[4] < 0x80);
                outState.SetButton("down", binaryPacket[4] > 0x80);
            }

            return outState.Build();
        }
    }
}