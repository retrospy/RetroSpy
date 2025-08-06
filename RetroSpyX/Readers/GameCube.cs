using System;
using System.Linq;

namespace RetroSpy.Readers
{
    public static class GameCube
    {
        private const int PACKET_SIZE = 64;
        private const int NICOHOOD_PACKET_SIZE = 8;

        private static bool adjustForNoStopBit = false;
        private static int noStopBitCount = 0;
        private static int noStopBitTotalCount = 0;

        private static byte currentMode = 0;
        private static int currentModeCount = 0;
        private static byte fixedMode = 0;
        private static bool useFixedMode = false;

        private static readonly string?[] BUTTONS = {
            null, null, null, "start", "y", "x", "b", "a", null, "l", "r", "z", "up", "down", "right", "left"
        };

        // Button order for the Nicohood Nintendo API
        // https://github.com/NicoHood/Nintendo
        // Each byte is reverse from the buttons above
        static readonly string?[] NICOHOOD_BUTTONS = {
            "a", "b", "x", "y", "start", null, null, null, "left", "right", "down", "up", "z", "r", "l", null
        };

        private static readonly string?[] KEYS =
        {
            null, null, null, null, null, null, "Home", "End",
            "PageUp", "PageDown", null, "ScrollLock", null, null, null, null,
            "K_A", "K_B", "C", "D", "E", "F", "G", "H",
            "I", "J", "K", "K_L", "M", "N", "O", "P",

            "Q", "K_R", "S", "T", "U", "V", "W", "K_X",
            "K_Y", "K_Z", "D1", "D2", "D3", "D4", "D5", "D6",
            "D7", "D8", "D9", "D0", "Minus", "Equals", "Yen", "LeftBracket",
            "RightBracket", "Semicolon", "Apostrophe", "LeftOfReturn", "Comma", "Period", "Slash", "JpSlash",

            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8",
            "F9", "F10", "F11", "F12", "Escape", "Insert", "Delete", "Grave",
            "Back", "Tab", null, "Capital", "LeftShift", "RightShift", "LeftControl", "LeftAlt",
            "LeftWindowsKey", "Space", "RightWindowsKey", "Applications", "K_left", "K_down", "K_up", "K_right",

            null, "Return", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
        };

        private static readonly string?[] FUNCTION_KEYS =
        {
            null, null, null, null, null, null, "Function", "Function",
            "Function", "Function", null, "Function", null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,

            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,

            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,

            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,

        };

        private static float ReadStick(byte input, float midVal = 128)
        {
            return (float)(input - midVal) / midVal;
        }

        private static float ReadTrigger(byte input, float maxVal = 256)
        {
            return (float)input / maxVal;
        }

        private static readonly byte[] keyboardData = new byte[3];

        private static long[] pollFormatCounts = new long[5];

        public static ControllerStateEventArgs? ReadFromSecondPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length == 3)
            {
                for (int i = 0; i < 3; ++i)
                {
                    keyboardData[i] = packet[i];
                }
            }

            return null;
        }

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length == 3)
            {
                ControllerStateBuilder state1 = new();

                for (int i = 0; i < KEYS.Length; ++i)
                {
                    if (KEYS[i] != null)
                    {
                        state1.SetButton(KEYS[i], false);
                    }
                }

                for (int i = 0; i < packet.Length; ++i)
                {
                    if (KEYS[packet[i]] != null)
                    {
                        state1.SetButton(KEYS[packet[i]], true);
                    }
                }

                state1.SetButton("Function", false);
                for (int i = 0; i < packet.Length; ++i)
                {
                    if (FUNCTION_KEYS[packet[i]] != null)
                    {
                        state1.SetButton(FUNCTION_KEYS[packet[i]], true);
                    }
                }

                return state1.Build();
            }

            if (packet.Length == NICOHOOD_PACKET_SIZE) // Packets are written as bytes when writing from the NicoHood API, so we're looking for a packet size of 8 (interpreted as bytes)
            {

                ControllerStateBuilder stateNico = new();

                for (int i = 0; i < 16; i++) // Handles the two button bytes
                {
                    if (string.IsNullOrEmpty(NICOHOOD_BUTTONS[i])) continue;
                    int bitPacket = (packet[i / 8] >> (i % 8)) & 0x1;
                    stateNico.SetButton(NICOHOOD_BUTTONS[i], bitPacket != 0x00);
                }

                stateNico.SetAnalog("lstick_x", ReadStick(packet[2]), packet[2]);
                stateNico.SetAnalog("lstick_y", ReadStick(packet[3]), packet[3]);
                stateNico.SetAnalog("cstick_x", ReadStick(packet[4]), packet[4]);
                stateNico.SetAnalog("cstick_y", ReadStick(packet[5]), packet[5]);
                stateNico.SetAnalog("trig_l", ReadTrigger(packet[6]), packet[6]);
                stateNico.SetAnalog("trig_r", ReadTrigger(packet[7]), packet[7]);

                return stateNico.Build();
            }
            
            byte[] data = new byte[PACKET_SIZE];

            // Add an addition 4 bytes for the poll mode and stop bit
            if (packet.Length == PACKET_SIZE || packet.Length == PACKET_SIZE - 8)
            {
                data = packet;
            }
            else if (packet.Length == PACKET_SIZE + 12 && (adjustForNoStopBit || packet[11] != 0))  // throw out if no stop bit
            {
                // Strip off poll mode, rumble mode and stop bit
                Array.Copy(packet, 12, data, 0, PACKET_SIZE);
                noStopBitTotalCount++;
                if (adjustForNoStopBit == false && noStopBitTotalCount >= 100)
                {
                    noStopBitCount = 0;
                    noStopBitTotalCount = 0;
                }
            }
            else if (packet.Length >= PACKET_SIZE)
            {
                if (packet[14] != 0)  // wii
                {
                    bool foundStopBit = false;
                    for (int i = 10; i >= 3; --i)
                    {
                        if (packet[i] != 0x00)
                        {
                            Array.Copy(packet, i + 1, data, 0, PACKET_SIZE - i);
                            foundStopBit = true;
                            break;
                        }
                    }

                    if (!foundStopBit)
                        return null;
                }
                else  // gamecube
                {
                    if (++noStopBitCount == 10)
                    {
                        adjustForNoStopBit = true;
                    }
                }

                return null;
            }
            else
                return null;

                ControllerStateBuilder state = new();

            for (int i = 0; i < BUTTONS.Length; ++i)
            {
                if (string.IsNullOrEmpty(BUTTONS[i]))
                {
                    continue;
                }

                state.SetButton(BUTTONS[i], data[i] != 0x00);
            }

            for (int i = 0; i < KEYS.Length; ++i)
            {
                if (KEYS[i] != null)
                {
                    state.SetButton(KEYS[i], false);
                }
            }

            for (int i = 0; i < keyboardData.Length; ++i)
            {
                if (KEYS[keyboardData[i]] != null)
                {
                    state.SetButton(KEYS[keyboardData[i]], true);
                }
            }

            state.SetButton("Function", false);
            for (int i = 0; i < keyboardData.Length; ++i)
            {
                if (FUNCTION_KEYS[keyboardData[i]] != null)
                {
                    state.SetButton(FUNCTION_KEYS[keyboardData[i]], true);
                }
            }

            state.SetAnalog("lstick_x", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length)), SignalTool.ReadByte(packet, BUTTONS.Length));
            state.SetAnalog("lstick_y", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 8)), SignalTool.ReadByte(packet, BUTTONS.Length + 8));

            int mode;
            if (packet.Length == PACKET_SIZE)
            {
                mode = 3;
            }
            else if (packet.Length == PACKET_SIZE - 8)
            {
                mode = 0;
            }
            else
            {
                mode = (packet[0] == 0x00 ? 0x00 : 0x04) | (packet[1] == 0x00 ? 0x00 : 0x02) | (packet[2] == 0x00 ? 0x00 : 0x01);
                if (currentMode == mode)
                {
                    currentModeCount++;
                    if (currentModeCount >= 5)
                    {
                        fixedMode = (byte)mode;
                        useFixedMode = true;
                    }
                }
                else
                {
                    currentMode = (byte)mode;
                    currentModeCount = 0;
                }
                if (useFixedMode == true)
                {
                    mode = fixedMode;
                }    
            }

            switch (mode)
            {
                case 0x00:
                    if (packet.Length > PACKET_SIZE - 8 && SignalTool.ReadByte(data, BUTTONS.Length + 40) != 0)
                        return null;
                    state.SetAnalog("cstick_x", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 16)), SignalTool.ReadByte(packet, BUTTONS.Length + 16));
                    state.SetAnalog("cstick_y", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 24)), SignalTool.ReadByte(packet, BUTTONS.Length + 24));
                    state.SetAnalog("trig_l", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 32, 4), 15), SignalTool.ReadByte(packet, BUTTONS.Length + 32, 4));
                    state.SetAnalog("trig_r", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 36, 4), 15), SignalTool.ReadByte(packet, BUTTONS.Length + 36, 4));
                    state.SetAnalog("analog_a", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 40, 4), 16), SignalTool.ReadByte(packet, BUTTONS.Length + 40, 4));
                    state.SetAnalog("analog_b", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 44, 4), 16), SignalTool.ReadByte(packet, BUTTONS.Length + 44, 4));
                    break;
                case 0x01:
                    return null;
                    //state.SetAnalog("cstick_x", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 16, 4), 8), SignalTool.ReadByte(packet, BUTTONS.Length + 16, 4));
                    //state.SetAnalog("cstick_y", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 20, 4), 8), SignalTool.ReadByte(packet, BUTTONS.Length + 20, 4));
                    //state.SetAnalog("trig_l", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 24)), SignalTool.ReadByte(packet, BUTTONS.Length + 24));
                    //state.SetAnalog("trig_r", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 32)), SignalTool.ReadByte(packet, BUTTONS.Length + 32));
                    //state.SetAnalog("analog_a", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 40, 4), 16), SignalTool.ReadByte(packet, BUTTONS.Length + 40, 4));
                    //state.SetAnalog("analog_b", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 44, 4), 16), SignalTool.ReadByte(packet, BUTTONS.Length + 44, 4));
                    //break;
                case 0x02:
                    return null;
                    //state.SetAnalog("cstick_x", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 16, 4), 8), SignalTool.ReadByte(packet, BUTTONS.Length + 16, 4));
                    //state.SetAnalog("cstick_y", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 20, 4), 8), SignalTool.ReadByte(packet, BUTTONS.Length + 20, 4));
                    //state.SetAnalog("trig_l", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 24, 4), 16), SignalTool.ReadByte(packet, BUTTONS.Length + 24, 4));
                    //state.SetAnalog("trig_r", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 28, 4), 16), SignalTool.ReadByte(packet, BUTTONS.Length + 28, 4));
                    //state.SetAnalog("analog_a", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 32), 15), SignalTool.ReadByte(packet, BUTTONS.Length + 32));
                    //state.SetAnalog("analog_b", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 40), 15), SignalTool.ReadByte(packet, BUTTONS.Length + 40));
                    //break;
                case 0x03:
                    state.SetAnalog("cstick_x", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 16)), SignalTool.ReadByte(packet, BUTTONS.Length + 16));
                    state.SetAnalog("cstick_y", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 24)), SignalTool.ReadByte(packet, BUTTONS.Length + 24));
                    state.SetAnalog("trig_l", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 32)), SignalTool.ReadByte(packet, BUTTONS.Length + 32));
                    state.SetAnalog("trig_r", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 40)), SignalTool.ReadByte(packet, BUTTONS.Length + 40));
                    break;
                case 0x04:
                    //state.SetAnalog("cstick_x", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 16)), SignalTool.ReadByte(packet, BUTTONS.Length + 16));
                    //state.SetAnalog("cstick_y", ReadStick(SignalTool.ReadByte(data, BUTTONS.Length + 24)), SignalTool.ReadByte(packet, BUTTONS.Length + 24));
                    //state.SetAnalog("analog_a", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 32)), SignalTool.ReadByte(packet, BUTTONS.Length + 32));
                    //state.SetAnalog("analog_a", ReadTrigger(SignalTool.ReadByte(data, BUTTONS.Length + 40)), SignalTool.ReadByte(packet, BUTTONS.Length + 40));
                    //break;
                default:
                    return null;
            }

            return state.Build();
        }
    }
}
