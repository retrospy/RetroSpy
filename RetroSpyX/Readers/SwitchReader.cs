using System;

namespace RetroSpy.Readers
{
    public static class SwitchReader
    {
        private const int PRO_PACKET_SIZE = 57;
        private const int POKKEN_PACKET_SIZE = 58;
        private const int POLISHED_PACKET_SIZE = 28;

        private static readonly string?[] PRO_BUTTONS = {
            "y", "x", "b", "a", null, null, "r", "zr", "-", "+", "rs", "ls", "home", "capture", null, null, "down", "up", "right", "left", null, null, "l", "zl"
        };

        private static readonly string?[] POKKEN_BUTTONS = {
            "y", "b", "a", "x", "l", "r", "zl", "zr", "-", "+", "ls", "rs", "home", "capture"
        };

        private static float ReadStick(byte input)
        {
            return input < 127 ? (float)input / 128 : (float)(255 - input) / -128;
        }

        private static float ReadPokkenStick(byte input, bool invert)
        {
            return invert ? -1.0f * ((float)(input - 128) / 128) : (float)(input - 128) / 128;
        }

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length < PRO_PACKET_SIZE)
            {
                return null;
            }

            byte[] polishedPacket = new byte[POLISHED_PACKET_SIZE];

            if (packet.Length == PRO_PACKET_SIZE)
            {
                for (int i = 0; i < 24; ++i)
                {
                    polishedPacket[i] = (byte)((packet[i] == 0x31) ? 1 : 0);
                }

                for (int i = 0; i < 4; ++i)
                {
                    polishedPacket[24 + i] = 0;
                    for (byte j = 0; j < 8; ++j)
                    {
                        polishedPacket[24 + i] |= (byte)((packet[24 + (i * 8) + j] == 0x30 ? 0 : 1) << j);
                    }
                }

                ControllerStateBuilder outState = new();

                for (int i = 0; i < PRO_BUTTONS.Length; ++i)
                {
                    if (string.IsNullOrEmpty(PRO_BUTTONS[i]))
                    {
                        continue;
                    }

                    outState.SetButton(PRO_BUTTONS[i], polishedPacket[i] != 0x00);
                }

                outState.SetAnalog("rstick_x", ReadStick(polishedPacket[26]), polishedPacket[26]);
                outState.SetAnalog("rstick_y", ReadStick(polishedPacket[27]), polishedPacket[27]);
                outState.SetAnalog("lstick_x", ReadStick(polishedPacket[24]), polishedPacket[24]);
                outState.SetAnalog("lstick_y", ReadStick(polishedPacket[25]), polishedPacket[25]);

                return outState.Build();
            }
            else if (packet.Length == POKKEN_PACKET_SIZE)
            {
                for (int i = 0; i < 16; ++i)
                {
                    polishedPacket[i] = (byte)((packet[i] == 0x31) ? 1 : 0);
                }

                polishedPacket[16] = 0;
                for (byte j = 0; j < 4; ++j)
                {
                    polishedPacket[16] |= (byte)((packet[16 + j] == 0x30 ? 0 : 1) << j);
                }

                for (int i = 0; i < 4; ++i)
                {
                    polishedPacket[17 + i] = 0;
                    for (byte j = 0; j < 8; ++j)
                    {
                        polishedPacket[17 + i] |= (byte)((packet[24 + (i * 8) + j] == 0x30 ? 0 : 1) << j);
                    }
                }

                ControllerStateBuilder outState = new();

                for (int i = 0; i < POKKEN_BUTTONS.Length; ++i)
                {
                    if (string.IsNullOrEmpty(POKKEN_BUTTONS[i]))
                    {
                        continue;
                    }

                    outState.SetButton(POKKEN_BUTTONS[i], polishedPacket[i] != 0x00);
                }

                switch (polishedPacket[16])
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

                outState.SetAnalog("lstick_x", ReadPokkenStick(polishedPacket[17], false), polishedPacket[17]);
                outState.SetAnalog("lstick_y", ReadPokkenStick(polishedPacket[18], true), polishedPacket[18]);
                outState.SetAnalog("rstick_x", ReadPokkenStick(polishedPacket[19], false), polishedPacket[19]);
                outState.SetAnalog("rstick_y", ReadPokkenStick(polishedPacket[20], true), polishedPacket[20]);

                return outState.Build();
            }

            return null;
        }
    }
}