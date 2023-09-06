//
// config_pico.h
//
// Author:
//       T.T <tt3333@tt-server.net>
//
// Copyright (c) 2022 RetroSpy Technologies
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#define PINC_READ( pin ) (gpio_get(pin))
#define PIND_READ( pin ) (gpio_get(pin))
#define PINB_READ( pin ) (gpio_get(pin+8))
#define READ_PORTD( mask ) ((gpio_get_all() << 2) & mask)
#define READ_PORTB( mask ) ((gpio_get_all() >> 8) & mask)

#define MICROSECOND_NOPS "nop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\n"
#define T_DELAY( ms ) delay(0)
#define A_DELAY( ms ) delay(0)

#define MODEPIN_SNES       10
#define MODEPIN_WII        9

#define SNES_LATCH         17
#define SNES_DATA          16
#define SNES_CLOCK         18

#define NES_LATCH          1
#define NES_CLOCK          4
#define NES_DATA           2
#define NES_DATA0          0
#define NES_DATA1          3

#define N64_PIN	           0

#define GC_PIN             3

#define CDI_IRPIN				15
#define CDI_RECVSER				0xFF
#define CDI_SENDSER				11
#define CDI_SENDSER_1			12

#define CD32_LATCH    5
#define CD32_DATA     7
#define CD32_CLOCK    6

#define SMSONGEN_INPUT_PIN_0    0
#define SMSONGEN_INPUT_PIN_1    1
#define SMSONGEN_INPUT_PIN_2    2
#define SMSONGEN_INPUT_PIN_3    3
#define SMSONGEN_INPUT_PIN_4    4
#define SMSONGEN_INPUT_PIN_5    5

#define PS_ATT             0
#define PS_CLOCK           1
#define PS_ACK             2
#define PS_CMD             3
#define PS_DATA            4

#define SS_SELECT0         4
#define SS_SEL             4
#define SS_SELECT1         5
#define SS_REQ             5
#define SS_ACK             6
#define SS_DATA0           0
#define SS_DATA1           1
#define SS_DATA2           2
#define SS_DATA3           3

#define FMTOWNS_MOUSE_STROBE	9
#define FMTOWNS_MOUSE_BUTTON_1	7
#define FMTOWNS_MOUSE_BUTTON_2	8

#define FASTRUN
