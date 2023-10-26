using System;

namespace RetroSpy.Readers
{

    public static class VSmile
    {
        private const int PACKET_SIZE = 10;

        private static readonly string?[] BUTTONS = {
            "red", "yellow", "blue", "green", "enter", "help", "exit", "learningzone", null, null
        };

        private static float ReadStick(byte input)
        {
            if (input >= 8)
                return (float)((8.0 - input) / 7.0);
            else
                return (float)(input / 7.0);
        }

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length == PACKET_SIZE)
            {
                ControllerStateBuilder state = new();

                for (int i = 0; i < BUTTONS.Length; ++i)
                {
                    if (string.IsNullOrEmpty(BUTTONS[i]))
                    {
                        continue;
                    }

                    state.SetButton(BUTTONS[i], packet[i] != 0x00);
                }

                state.SetAnalog("lstick_x", ReadStick((byte)(packet[8] >> 4)), packet[8] >> 4);
                state.SetAnalog("lstick_y", ReadStick((byte)(packet[9] >> 4)), packet[9] >> 4);

                return state.Build();
            }

            return null;
        }
    }
}
