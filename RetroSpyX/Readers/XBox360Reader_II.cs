using System;
using System.Text;

namespace RetroSpy.Readers
{
    public static class XBox360Reader_II
    {
        private const int PACKET_SIZE = 41;

        private static readonly string?[] BUTTONS = {
            "up", "down", "left", "right", "start", "back", "l3", "r3", "lb", "rb", "xbox", null, "a", "b", "x", "y"
        };

        private static float ReadTrigger(byte input)
        {
            return (float)input / 255;
        }

        private static float ReadStick(short input)
        {
            return (float)input / 32768;
        }

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

            outState.SetButton(BUTTONS[0], (binaryPacket[2] & 1) != 0x00);
            outState.SetButton(BUTTONS[1], (binaryPacket[2] & 2) != 0x00);
            outState.SetButton(BUTTONS[2], (binaryPacket[2] & 4) != 0x00);
            outState.SetButton(BUTTONS[3], (binaryPacket[2] & 8) != 0x00);
            outState.SetButton(BUTTONS[4], (binaryPacket[2] & 16) != 0x00);
            outState.SetButton(BUTTONS[5], (binaryPacket[2] & 32) != 0x00);
            outState.SetButton(BUTTONS[6], (binaryPacket[2] & 64) != 0x00);
            outState.SetButton(BUTTONS[7], (binaryPacket[2] & 128) != 0x00);

            outState.SetButton(BUTTONS[8], (binaryPacket[3] & 1) != 0x00);
            outState.SetButton(BUTTONS[9], (binaryPacket[3] & 2) != 0x00);
            outState.SetButton(BUTTONS[10], (binaryPacket[3] & 4) != 0x00);
            outState.SetButton(BUTTONS[12], (binaryPacket[3] & 16) != 0x00);
            outState.SetButton(BUTTONS[13], (binaryPacket[3] & 32) != 0x00);
            outState.SetButton(BUTTONS[14], (binaryPacket[3] & 64) != 0x00);
            outState.SetButton(BUTTONS[15], (binaryPacket[3] & 128) != 0x00);

            outState.SetAnalog("trig_l", ReadTrigger(binaryPacket[4]), binaryPacket[4]);
            outState.SetAnalog("trig_r", ReadTrigger(binaryPacket[5]), binaryPacket[5]);

            outState.SetAnalog("rstick_y", ReadStick((short)(binaryPacket[13] << 8 | binaryPacket[12])), (short)(binaryPacket[13] << 8 | binaryPacket[12]));
            outState.SetAnalog("rstick_x", ReadStick((short)(binaryPacket[11] << 8 | binaryPacket[10])), (short)(binaryPacket[10] << 8 | binaryPacket[10]));
            outState.SetAnalog("lstick_y", ReadStick((short)(binaryPacket[9] << 8 | binaryPacket[8])), (short)(binaryPacket[9] << 8 | binaryPacket[8]));
            outState.SetAnalog("lstick_x", ReadStick((short)(binaryPacket[7] << 8 | binaryPacket[6])), (short)(binaryPacket[7] << 8 | binaryPacket[6]));

            return outState.Build();
        }
    }
}