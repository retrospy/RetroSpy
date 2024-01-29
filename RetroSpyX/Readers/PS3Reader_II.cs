using System;
using System.Text;

namespace RetroSpy.Readers
{
    public static class PS3Reader_II
    {
        private const int PACKET_SIZE = 99;
        private const int PS4_PACKET_SIZE = 129;

        private static readonly string[] BUTTONS = {
            "select", "lstick", "rstick", "start", "up", "right", "down", "left", "l2", "r2", "l1", "r1", "triangle", "circle", "x", "square", "ps"
        };

        private static readonly string[] PS4_BUTTONS = {
            "up", "down", "left", "right", "x", "circle", "square", "triangle", "l1", "l2", "lstick", "r1", "r2", "rstick", "select", "start", "ps"
        };

        private static float ReadAnalogButton(byte input)
        {
            return (float)input / 256;
        }

        private static float ReadStick(byte input)
        {
            return (float)(input - 128) / 128;
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

            if (packet.Length == PACKET_SIZE)
            {
                byte[] binaryPacket = StringToByteArray(Encoding.UTF8.GetString(packet, 0, packet.Length).Trim());

                // PS1 Games have a weird packet that I need to filter out
                if (binaryPacket[10] != 0)
                    return null;

                ControllerStateBuilder outState = new();

                for (int i = 2; i < 4; ++i)
                {
                    for (int j = 0; j < 8; ++j)
                    {
                        outState.SetButton(BUTTONS[((i - 2) * 8) + j], (binaryPacket[i] & (1 << j)) != 0x00);
                    }
                }
                outState.SetButton(BUTTONS[16], (binaryPacket[4] & 0x01) != 0x00);


                outState.SetAnalog("lstick_x", ReadStick(binaryPacket[6]), binaryPacket[6]);
                outState.SetAnalog("lstick_y", ReadStick(binaryPacket[7]), binaryPacket[7]);
                outState.SetAnalog("rstick_x", ReadStick(binaryPacket[8]), binaryPacket[8]);
                outState.SetAnalog("rstick_y", ReadStick(binaryPacket[9]), binaryPacket[9]);

                outState.SetAnalog("analog_up", ReadAnalogButton(binaryPacket[14]), binaryPacket[14]);
                outState.SetAnalog("analog_right", ReadAnalogButton(binaryPacket[15]), binaryPacket[15]);
                outState.SetAnalog("analog_down", ReadAnalogButton(binaryPacket[16]), binaryPacket[16]);
                outState.SetAnalog("analog_left", ReadAnalogButton(binaryPacket[17]), binaryPacket[17]);
                outState.SetAnalog("l_trig", ReadAnalogButton(binaryPacket[18]), binaryPacket[18]);
                outState.SetAnalog("r_trig", ReadAnalogButton(binaryPacket[19]), binaryPacket[19]);
                outState.SetAnalog("analog_l1", ReadAnalogButton(binaryPacket[20]), binaryPacket[20]);
                outState.SetAnalog("analog_r1", ReadAnalogButton(binaryPacket[21]), binaryPacket[21]);
                outState.SetAnalog("analog_triangle", ReadAnalogButton(binaryPacket[22]), binaryPacket[22]);
                outState.SetAnalog("analog_circle", ReadAnalogButton(binaryPacket[23]), binaryPacket[23]);
                outState.SetAnalog("analog_x", ReadAnalogButton(binaryPacket[24]), binaryPacket[24]);
                outState.SetAnalog("analog_square", ReadAnalogButton(binaryPacket[25]), binaryPacket[25]);

                return outState.Build();
            }
            else if (packet.Length == PS4_PACKET_SIZE)  //PS 4 Controller
            {
                byte[] binaryPacket = StringToByteArray(Encoding.UTF8.GetString(packet, 0, packet.Length).Trim());

                ControllerStateBuilder outState = new();

                outState.SetButton(PS4_BUTTONS[0], (binaryPacket[5] & 0x0F) == 0x07
                                || (binaryPacket[5] & 0x0F) == 0x01 || (binaryPacket[5] & 0x0F) == 0x00);
                outState.SetButton(PS4_BUTTONS[1], (binaryPacket[5] & 0x0F) == 0x05
                                                || (binaryPacket[5] & 0x0F) == 0x04 || (binaryPacket[5] & 0x0F) == 0x03);
                outState.SetButton(PS4_BUTTONS[2], (binaryPacket[5] & 0x0F) == 0x07
                                                || (binaryPacket[5] & 0x0F) == 0x06 || (binaryPacket[5] & 0x0F) == 0x05);
                outState.SetButton(PS4_BUTTONS[3], (binaryPacket[5] & 0x0F) == 0x03
                                                || (binaryPacket[5] & 0x0F) == 0x02 || (binaryPacket[5] & 0x0F) == 0x01);

                outState.SetButton(PS4_BUTTONS[4], (binaryPacket[5] & 0b00100000) != 0);
                outState.SetButton(PS4_BUTTONS[5], (binaryPacket[5] & 0b01000000) != 0);
                outState.SetButton(PS4_BUTTONS[6], (binaryPacket[5] & 0b00010000) != 0);
                outState.SetButton(PS4_BUTTONS[7], (binaryPacket[5] & 0b10000000) != 0);

                outState.SetButton(PS4_BUTTONS[8], (binaryPacket[6] & 0b00000001) != 0);
                outState.SetButton(PS4_BUTTONS[9], (binaryPacket[6] & 0b00000100) != 0);
                outState.SetButton(PS4_BUTTONS[10], (binaryPacket[6] & 0b01000000) != 0);
                outState.SetButton(PS4_BUTTONS[11], (binaryPacket[6] & 0b00000010) != 0);
                outState.SetButton(PS4_BUTTONS[12], (binaryPacket[6] & 0b00001000) != 0);
                outState.SetButton(PS4_BUTTONS[13], (binaryPacket[6] & 0b10000000) != 0);

                outState.SetButton(PS4_BUTTONS[14], (binaryPacket[6] & 0b00010000) != 0);
                outState.SetButton(PS4_BUTTONS[15], (binaryPacket[6] & 0b00100000) != 0);
                outState.SetButton(PS4_BUTTONS[16], (binaryPacket[7] & 0b00000001) != 0);

                outState.SetAnalog("lstick_x", ReadStick(binaryPacket[1]), binaryPacket[1]);
                outState.SetAnalog("lstick_y", ReadStick(binaryPacket[2]), binaryPacket[2]);
                outState.SetAnalog("rstick_x", ReadStick(binaryPacket[3]), binaryPacket[3]);
                outState.SetAnalog("rstick_y", ReadStick(binaryPacket[4]), binaryPacket[4]);

                outState.SetAnalog("l_trig", ReadStick(binaryPacket[8]), binaryPacket[3]);
                outState.SetAnalog("r_trig", ReadStick(binaryPacket[9]), binaryPacket[4]);

                outState.SetAnalog("analog_up", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_right", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_down", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_left", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_l1", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_r1", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_triangle", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_circle", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_x", ReadAnalogButton(0), 0);
                outState.SetAnalog("analog_square", ReadAnalogButton(0), 0);

                return outState.Build();
            }

            return null;

      /*   for (int i = 0; i < 24; ++i)
            {
                polishedPacket[i] = (byte)((packet[i] == 0x31) ? 1 : 0);
            }

            for (int i = 0; i < 16; ++i)
            {
                polishedPacket[24 + i] = 0;
                for (byte j = 0; j < 8; ++j)
                {
                    polishedPacket[24 + i] |= (byte)((packet[24 + (i * 8) + j] == 0x30 ? 0 : 1) << j);
                }
            }

            ControllerStateBuilder outState = new();

            for (int i = 0; i < BUTTONS.Length; ++i)
            {
                if (string.IsNullOrEmpty(BUTTONS[i]))
                {
                    continue;
                }

                outState.SetButton(BUTTONS[i], polishedPacket[i] != 0x00);
            }

            outState.SetAnalog("rstick_x", ReadStick(polishedPacket[26]), polishedPacket[26]);
            outState.SetAnalog("rstick_y", ReadStick(polishedPacket[27]), polishedPacket[27]);
            outState.SetAnalog("lstick_x", ReadStick(polishedPacket[24]), polishedPacket[24]);
            outState.SetAnalog("lstick_y", ReadStick(polishedPacket[25]), polishedPacket[25]);

            outState.SetAnalog("analog_up", ReadAnalogButton(polishedPacket[28]), polishedPacket[28]);
            outState.SetAnalog("analog_right", ReadAnalogButton(polishedPacket[29]), polishedPacket[29]);
            outState.SetAnalog("analog_down", ReadAnalogButton(polishedPacket[30]), polishedPacket[30]);
            outState.SetAnalog("analog_left", ReadAnalogButton(polishedPacket[31]), polishedPacket[31]);
            outState.SetAnalog("l_trig", ReadAnalogButton(polishedPacket[32]), polishedPacket[32]);
            outState.SetAnalog("r_trig", ReadAnalogButton(polishedPacket[33]), polishedPacket[33]);
            outState.SetAnalog("analog_l1", ReadAnalogButton(polishedPacket[34]), polishedPacket[34]);
            outState.SetAnalog("analog_r1", ReadAnalogButton(polishedPacket[35]), polishedPacket[35]);
            outState.SetAnalog("analog_triangle", ReadAnalogButton(polishedPacket[36]), polishedPacket[36]);
            outState.SetAnalog("analog_circle", ReadAnalogButton(polishedPacket[37]), polishedPacket[37]);
            outState.SetAnalog("analog_x", ReadAnalogButton(polishedPacket[38]), polishedPacket[38]);
            outState.SetAnalog("analog_square", ReadAnalogButton(polishedPacket[39]), polishedPacket[39]);
      */
            
        }
    }
}