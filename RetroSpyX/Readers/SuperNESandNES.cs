using System;
using System.Text;

namespace RetroSpy.Readers
{
    public static class SuperNESandNES
    {
        private static ControllerStateEventArgs? ReadPacketButtons(byte[] packet, string?[] buttons, byte mask = 0xFF)
        {
            if (packet == null || packet.Length < buttons.Length)
            {
                return null;
            }

            ControllerStateBuilder state = new();

            for (int i = 0; i < buttons.Length; ++i)
            {
                if (string.IsNullOrEmpty(buttons[i]))
                {
                    continue;
                }

                state.SetButton(buttons[i], (packet[i] & mask) != 0x00);
            }

            return state.Build();
        }

        private static ControllerStateEventArgs? ReadPacketButtons_ascii(byte[]? packet, string?[] buttons)
        {
            if (packet == null || packet.Length < buttons.Length)
            {
                return null;
            }

            ControllerStateBuilder state = new();

            for (int i = 0; i < buttons.Length; ++i)
            {
                if (string.IsNullOrEmpty(buttons[i]))
                {
                    continue;
                }

                state.SetButton(buttons[i], packet[i] != '0');
            }

            return state.Build();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static ControllerStateEventArgs? ReadPacketButtonsPS_binary(byte[]? packet, string?[] buttons)
        {
            if (packet == null)
            {
                return null;
            }

            byte[] binaryPacket = StringToByteArray(Encoding.UTF8.GetString(packet, 0, packet.Length).Trim());
       
            if ((packet.Length * 8) < buttons.Length)
                return null;

            ControllerStateBuilder state = new();

            for (int i = 0; i < buttons.Length; ++i)
            {
                if (string.IsNullOrEmpty(buttons[i]))
                {
                    continue;
                }

                if(i == 10 || i == 12)
                {
                    state.SetButton(buttons[i], (binaryPacket[i / 8] & (1 << (i % 8))) == 0 && (binaryPacket[i / 8] & (1 << ((i+1) % 8))) == 0);
                }
                else
                    state.SetButton(buttons[i], (binaryPacket[i/8] & (1 << (i%8)))  != 0);
            }

            return state.Build();
        }

        private static readonly string?[] BUTTONS_NES = {
            "a", "b", "select", "start", "up", "down", "left", "right", "2", "1", "5", "9", "6", "10", "11", "7", "4", "3", "12", "8", null, null, null, null
        };

        private static readonly string?[] BUTTONS_NES_BACKCOMPAT = {
            "a", "b", "select", "start", "up", "down", "left", "right"
        };

        private static readonly string?[] BUTTONS_NES_POWERGLOVE = {
            "a", "b", "select", "start", "up", "down", "left", "right", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" ,"enter", "center"
        };

        private static readonly string?[] BUTTONS_NES_POWERGLOVE_FINGERS = {
            "thumb", "index", "middle", "ring"
        };

        private static readonly string?[] BUTTONS_SNES = {
            "b", "y", "select", "start", "up", "down", "left", "right", "a", "x", "l", "r", null, null, null, null
        };

        private static readonly string?[] BUTTONS_SNES_NTTDATA = {
            "b", "y", "select", "start", "up", "down", "left", "right", "a", "x", "l", "r", null, null, null, null, "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "star", "pound", "dot", "c", null, "end"
        };

        private static readonly string?[] BUTTONS_INTELLIVISION = {
            "n", "nne", "ne", "ene", "e", "ese", "se", "sse", "s", "ssw", "sw", "wsw", "w", "wnw", "nw", "nnw", "1", "2", "3", "4", "5", "6", "7", "8", "9", "clear", "0", "enter", "topleft", "topright", "bottomleft", "bottomright"
        };

        private static readonly string?[] BUTTONS_PSCLASSIC =
        {
            "r1", "l1", "r2", "l2", "square", "x", "circle", "triangle", null, null, "down", "up", "right", "left", "start", "select"
        };

        private static readonly string?[] BUTTONS_PSCLASSIC_II =
{
            "triangle", "circle", "x", "square", "l2", "r2", "l1", "r1", "select", "start", "left", "right", "up", "down", null, null
        };

        private static readonly string?[] BUTTONS_ATARI5200 =
        {
            "start", "pause", "reset", "1", "2", "3", "4", "5", "6", "7", "8", "9", "star", "0", "pound", "trigger", "fire", null, null
        };

        private static readonly string?[] BUTTONS_FMTOWNS =
        {
            "up", "down", "left", "right", null, "a", "b", null, null, "select", "run"
        };

        private static readonly string?[] BUTTONS_PCFX =
        {
            null, "1", "2", "3", "4", "5", "6", "select", "run", "up", "right", "down", "left", "mode1", null, "mode2"
        };

        private static readonly string?[] BUTTONS_VIRTUALBOY =
{
            "r_down", "r_left", "select", "start", "up", "down", "left", "right", "r_right", "r_up", "lt", "rt", "b", "a", null, null
        };

        private static readonly string?[] BUTTONS_NUON =
        {
            "CD", "A", "start", "nuon", "down", "left", "up", "right", null, null, "LT", "RT", "B", "CL", "CU", "CR"
        };

        public static ControllerStateEventArgs? ReadFromPacketIntellivision(byte[]? packet)
        {
            return packet == null ? throw new ArgumentNullException("ReadFromPacketIntellivision(packet)") : ReadPacketButtons(packet, BUTTONS_INTELLIVISION);
        }

        public static ControllerStateEventArgs? ReadFromPacketNES(byte[]? packet)
        {
            return packet == null
                ? throw new ArgumentNullException(nameof(packet))
                : packet.Length == 80
                ? ProcessPowerGlove(packet)
                : ReadPacketButtons(packet, packet.Length == 8 ? BUTTONS_NES_BACKCOMPAT : BUTTONS_NES);
        }

        public static ControllerStateEventArgs ProcessPowerGlove(byte[] packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            ControllerStateBuilder state = new();

            sbyte x = (sbyte)SignalTool.ReadByte(packet, 8, 8);
            state.SetAnalog("x", x / 128.0f, x);

            sbyte y = (sbyte)SignalTool.ReadByte(packet, 16, 8);
            state.SetAnalog("y", y / 128.0f, y);

            sbyte z = (sbyte)SignalTool.ReadByte(packet, 24, 8);
            state.SetAnalog("z", z / 128.0f, z);

            byte rotation = SignalTool.ReadByte(packet, 32, 8);
            state.SetAnalog("rotation", rotation / 11.0f, rotation);

            byte[] fingers = new byte[4];
            for (int i = 0; i < 4; ++i)
            {
                fingers[i] = 0;
                for (int j = 0; j < 2; ++j)
                {
                    fingers[i] |= (byte)(packet[40 + (i * 2) + j] == 0 ? 0 : (1 << j));
                }
            }
            for (int i = 0; i < 4; ++i)
            {
                state.SetAnalog(BUTTONS_NES_POWERGLOVE_FINGERS[i], fingers[i] / 4.0f, fingers[i]);
            }

            byte buttons = SignalTool.ReadByte(packet, 48, 8);
            for (int i = 0; i < BUTTONS_NES_POWERGLOVE.Length; ++i)
            {
                state.SetButton(BUTTONS_NES_POWERGLOVE[i], false);
            }
            switch (buttons)
            {
                case 0x00:
                    state.SetButton("0", true);
                    state.SetButton("center", true);
                    break;
                case 0x01:
                    state.SetButton("1", true);
                    break;
                case 0x02:
                    state.SetButton("2", true);
                    break;
                case 0x03:
                    state.SetButton("3", true);
                    break;
                case 0x04:
                    state.SetButton("4", true);
                    break;
                case 0x05:
                    state.SetButton("5", true);
                    break;
                case 0x06:
                    state.SetButton("6", true);
                    break;
                case 0x07:
                    state.SetButton("7", true);
                    break;
                case 0x08:
                    state.SetButton("8", true);
                    break;
                case 0x09:
                    state.SetButton("9", true);
                    break;
                case 0x0A:
                    state.SetButton("a", true);
                    break;
                case 0x0B:
                    state.SetButton("b", true);
                    break;
                case 0x0C:
                    state.SetButton("left", true);
                    break;
                case 0x0D:
                    state.SetButton("up", true);
                    break;
                case 0x0E:
                    state.SetButton("down", true);
                    break;
                case 0x0F:
                    state.SetButton("right", true);
                    break;
                case 0x80:
                    state.SetButton("enter", true);
                    break;
                case 0x82:
                    state.SetButton("start", true);
                    break;
                case 0x83:
                    state.SetButton("select", true);
                    break;
                default:
                    break;
            }

            return state.Build();
        }

        public static ControllerStateEventArgs? ReadFromPacketVB(byte[]? packet)
        {
            return packet == null ? throw new ArgumentNullException(nameof(packet)) : ReadPacketButtons(packet, BUTTONS_VIRTUALBOY);
        }

        public static ControllerStateEventArgs? ReadFromPacketNuon(byte[]? packet)
        {
            return packet == null ? throw new ArgumentNullException(nameof(packet)) : ReadPacketButtons(packet, BUTTONS_NUON, 0x04);
        }

        public static ControllerStateEventArgs? ReadFromPacketPCFX(byte[]? packet)
        {
            return packet == null
                ? throw new ArgumentNullException(nameof(packet))
                : packet.Length != BUTTONS_PCFX.Length ? null : ReadPacketButtons(packet, BUTTONS_PCFX);
        }

        public static ControllerStateEventArgs? ReadFromPacketPSClassic(byte[]? packet)
        {
            return packet == null ? throw new ArgumentNullException(nameof(packet)) : ReadPacketButtons_ascii(packet, BUTTONS_PSCLASSIC);
        }

        public static ControllerStateEventArgs? ReadFromPacketPSClassic_II(byte[]? packet)
        {
            return packet == null ? throw new ArgumentNullException(nameof(packet)) : ReadPacketButtonsPS_binary(packet, BUTTONS_PSCLASSIC_II);
        }

        public static ControllerStateEventArgs? ReadFromPacketSNES(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length < BUTTONS_SNES.Length)
            {
                return null;
            }

            ControllerStateBuilder state = new();

            if (packet.Length == 32 && packet[15] == 0x00 && packet[13] != 0x00)
            {
                for (int i = 0; i < BUTTONS_SNES_NTTDATA.Length; ++i)
                {
                    if (string.IsNullOrEmpty(BUTTONS_SNES_NTTDATA[i]))
                    {
                        continue;
                    }

                    state.SetButton(BUTTONS_SNES_NTTDATA[i], packet[i] != 0x00);
                }
            }
            else
            {
                for (int i = 0; i < BUTTONS_SNES.Length; ++i)
                {
                    if (string.IsNullOrEmpty(BUTTONS_SNES[i]))
                    {
                        continue;
                    }

                    state.SetButton(BUTTONS_SNES[i], packet[i] != 0x00);
                }

                if (packet.Length == 32 && packet[15] != 0x00)
                {
                    float y = (float)(SignalTool.ReadByte(packet, 17, 7, 0x1) * ((packet[16] & 0x1) != 0 ? 1 : -1)) / 127;
                    float x = (float)(SignalTool.ReadByte(packet, 25, 7, 0x1) * ((packet[24] & 0x1) != 0 ? -1 : 1)) / 127;
                    SignalTool.SetMouseProperties(x, y, SignalTool.ReadByte(packet, 25, 7, 0x1) * ((packet[24] & 0x1) != 0 ? -1 : 1), SignalTool.ReadByte(packet, 17, 7, 0x1) * ((packet[16] & 0x1) != 0 ? 1 : -1), state);
                }
            }

            return state.Build();
        }

        public static ControllerStateEventArgs? ReadFromPacketJaguar(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length < 4)
            {
                return null;
            }

            ControllerStateBuilder state = new();

            state.SetButton("pause", (packet[0] & 0b00000100) == 0x00);
            state.SetButton("a", (packet[0] & 0b00001000) == 0x00);
            state.SetButton("right", (packet[0] & 0b00010000) == 0x00);
            state.SetButton("left", (packet[0] & 0b00100000) == 0x00);
            state.SetButton("down", (packet[0] & 0b01000000) == 0x00);
            state.SetButton("up", (packet[0] & 0b10000000) == 0x00);

            state.SetButton("b", (packet[1] & 0b00001000) == 0x00);
            state.SetButton("1", (packet[1] & 0b00010000) == 0x00);
            state.SetButton("4", (packet[1] & 0b00100000) == 0x00);
            state.SetButton("l", (packet[1] & 0b00100000) == 0x00);
            state.SetButton("7", (packet[1] & 0b01000000) == 0x00);
            state.SetButton("x", (packet[1] & 0b01000000) == 0x00);
            state.SetButton("star", (packet[1] & 0b10000000) == 0x00);

            state.SetButton("c", (packet[2] & 0b00001000) == 0x00);
            state.SetButton("2", (packet[2] & 0b00010000) == 0x00);
            state.SetButton("5", (packet[2] & 0b00100000) == 0x00);
            state.SetButton("8", (packet[2] & 0b01000000) == 0x00);
            state.SetButton("y", (packet[2] & 0b01000000) == 0x00);
            state.SetButton("0", (packet[2] & 0b10000000) == 0x00);

            state.SetButton("option", (packet[3] & 0b00001000) == 0x00);
            state.SetButton("3", (packet[3] & 0b00010000) == 0x00);
            state.SetButton("6", (packet[3] & 0b00100000) == 0x00);
            state.SetButton("r", (packet[3] & 0b00100000) == 0x00);
            state.SetButton("9", (packet[3] & 0b01000000) == 0x00);
            state.SetButton("z", (packet[3] & 0b01000000) == 0x00);
            state.SetButton("pound", (packet[3] & 0b10000000) == 0x00);

            return state.Build();
        }

        private static readonly string?[] SCANCODES_FMTOWNS =
        {
            null, "ESC", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "-", "^", "yen", "Backspace",
            "Tab", "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "@", "[", "Enter", "A", "S",
            "D", "F", "G", "H", "J", "K", "L", ";", ":", "]", "Z", "X", "C", "V", "B", "N",
            "M", ",", ".", "/", "quote", "Spacebar", "*", "divide", "+", "subtract", "Num7", "Num8", "Num9", "=", "Num4", "Num5",
            "Num6", null, "Num1", "Num2", "Num3", "NumEnter", "Num0", "Num.", "DUP", null, "000", "EL", null, "Up", "Home", "Left",
            "Down", "Right", "CTRL", "SHIFT", null, "CAP", "BottomLeft", "BelowSpace1", "BelowSpace2", "RightOfSpacebar1",
                             "RightOfSpacebar2", "PF12", "ALT", "PF1", "PF2", "PF3",
            "PF4", "PF5", "PF6", "PF7", "PF8", "PF9", "PF10", null, null, "PF11", null, "UpperLeftOfHome", "AboveHome", "UpperRightOfHome", "LeftOfHome", null,
            "RightOfHome", "LowerLeftOfHome", "RightOfSpacebar3", "BelowArrows", "PF13", "PF14", "PF15", "PF16", "PF17", "PF18", "PF19", "PF20", "Pause",
                             "Copy", null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null, null, null, "SYSREQ", null, null,
            "ScrollLock", "SysHome", "End", null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, "EXT1", null, null, null, "EXT2", null, null, null, null, null, null, null
        };

        public static ControllerStateEventArgs? ReadFromPacketFMTowns(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length != 9 && packet.Length != 70)
            {
                return null;
            }

            if (packet.Length == 9)
            {
                byte[] polishedPacket = new byte[BUTTONS_FMTOWNS.Length];

                if (packet[0] != 0 && packet[1] != 0)
                {
                    packet[0] = packet[1] = 0;
                    polishedPacket[9] = 1;
                }

                if (packet[2] != 0 && packet[3] != 0)
                {
                    packet[2] = packet[3] = 0;
                    polishedPacket[10] = 1;
                }

                for (int i = 0; i < packet.Length; ++i)
                {
                    polishedPacket[i] = packet[i];
                }

                return ReadPacketButtons(polishedPacket, BUTTONS_FMTOWNS);
            }
            else
            {
                int j = 0;
                byte[] reconstructedPacket = new byte[34];
                for (int i = 0; i < 34; ++i)
                {
                    reconstructedPacket[i] = (byte)((packet[j] >> 4) | packet[j + 1]);
                    j += 2;
                }

                byte[] polishedPacket = new byte[256];

                for (int i = 0; i < 32; ++i)
                {
                    for (int k = 0; k < 8; ++k)
                    {
                        polishedPacket[(i * 8) + k] = (byte)((reconstructedPacket[i] & (1 << k)) != 0 ? 1 : 0);
                    }
                }

                ControllerStateBuilder state = new();

                for (int i = 0; i < SCANCODES_FMTOWNS.Length; ++i)
                {
                    if (string.IsNullOrEmpty(SCANCODES_FMTOWNS[i]))
                    {
                        continue;
                    }

                    state.SetButton(SCANCODES_FMTOWNS[i], polishedPacket[i] != 0x00);
                }

                SignalTool.SetMouseProperties(((sbyte)reconstructedPacket[33]) / -128.0f, ((sbyte)reconstructedPacket[32]) / 128.0f, reconstructedPacket[33], reconstructedPacket[32], state);

                state.SetButton("left", packet[68] == 1);
                state.SetButton("right", packet[69] == 1);

                return state.Build();
            }
        }

        private static float atari5200_y;
        private static int atari5200_yRaw;

        public static ControllerStateEventArgs? ReadFromPacketAtari52002(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length != BUTTONS_ATARI5200.Length)
            {
                return null;
            }

            atari5200_y = (((packet[17] >> 4) | (packet[18])) - 128.0f) / 128.0f;
            atari5200_yRaw = (packet[17] >> 4) | (packet[18]);
            return null;
        }

        public static ControllerStateEventArgs? ReadFromPacketAtari52001(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length != BUTTONS_ATARI5200.Length)
            {
                return null;
            }

            ControllerStateBuilder state = new();
            for (int i = 0; i < BUTTONS_ATARI5200.Length; ++i)
            {
                if (string.IsNullOrEmpty(BUTTONS_ATARI5200[i]))
                {
                    continue;
                }

                state.SetButton(BUTTONS_ATARI5200[i], packet[i] == 0x00);
            }

            state.SetAnalog("x", (((packet[17] >> 4) | (packet[18])) - 128.0f) / -128.0f, (packet[17] >> 4) | (packet[18]));
            state.SetAnalog("y", atari5200_y, atari5200_yRaw);

            return state.Build();
        }
    }
}