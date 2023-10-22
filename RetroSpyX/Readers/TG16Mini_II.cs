using System;
using System.Text;

namespace RetroSpy.Readers
{
    public static class Tg16Mini_II
    {
        private const int PACKET_SIZE = 17;

        private static readonly string[] BUTTONS = {
            "2", "1", "select", "run", "up", "right", "down", "left"
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

            outState.SetButton(BUTTONS[0], (binaryPacket[0] & 0b00000010) != 0);
            outState.SetButton(BUTTONS[1], (binaryPacket[0] & 0b00000100) != 0);
            outState.SetButton(BUTTONS[2], (binaryPacket[1] & 0b00000001) != 0);
            outState.SetButton(BUTTONS[3], (binaryPacket[1] & 0b00000010) != 0);

            switch (binaryPacket[2])
            {
                case 0:
                    outState.SetButton("up", true);
                    outState.SetButton("down", false);
                    outState.SetButton("left", false);
                    outState.SetButton("right", false);
                    break;

                case 1:
                    outState.SetButton("up", true);
                    outState.SetButton("right", true);
                    outState.SetButton("down", false);
                    outState.SetButton("left", false);
                    break;

                case 2:
                    outState.SetButton("right", true);
                    outState.SetButton("down", false);
                    outState.SetButton("left", false);
                    outState.SetButton("up", false);
                    break;

                case 3:
                    outState.SetButton("right", true);
                    outState.SetButton("down", true);
                    outState.SetButton("up", false);
                    outState.SetButton("left", false);
                    break;

                case 4:
                    outState.SetButton("down", true);
                    outState.SetButton("up", false);
                    outState.SetButton("left", false);
                    outState.SetButton("right", false);
                    break;

                case 5:
                    outState.SetButton("left", true);
                    outState.SetButton("down", true);
                    outState.SetButton("right", false);
                    outState.SetButton("up", false);
                    break;

                case 6:
                    outState.SetButton("right", false);
                    outState.SetButton("down", false);
                    outState.SetButton("up", false);
                    outState.SetButton("left", true);
                    break;

                case 7:
                    outState.SetButton("up", true);
                    outState.SetButton("left", true);
                    outState.SetButton("right", false);
                    outState.SetButton("down", false);
                    break;

                default:
                    outState.SetButton("up", false);
                    outState.SetButton("left", false);
                    outState.SetButton("right", false);
                    outState.SetButton("down", false);
                    break;
            }

            return outState.Build();
        }
    }
}
