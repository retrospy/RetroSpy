//
// config_every.h
//
// Author:
//       Christopher "Zoggins" Mallery <zoggins@retro-spy.com>
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

#define NOT_CONNECTED	   NA

#define DIGITAL_PIN_17	   2
#define DIGITAL_PIN_16	   3
#define DIGITAL_PIN_15	   4
#define DIGITAL_PIN_14	   5
#define DIGITAL_PIN_20	   6
#define DIGITAL_PIN_21	   7

#define DIGITAL_PIN_02	   0
#define DIGITAL_PIN_07	   1
#define DIGITAL_PIN_18	   2
#define DIGITAL_PIN_19	   3

#define MODEPIN_SNES       3
#define MODEPIN_N64        4
#define MODEPIN_GC         5
//#define MODEPIN_X        6
//#define MODEPIN_Y        8

#define N64_PIN	           DIGITAL_PIN_17

#define SNES_LATCH         DIGITAL_PIN_16
#define SNES_DATA          DIGITAL_PIN_15
#define SNES_CLOCK         DIGITAL_PIN_20

#define NES_LATCH          DIGITAL_PIN_16
#define NES_CLOCK          DIGITAL_PIN_20
#define NES_DATA           DIGITAL_PIN_15
#define NES_DATA0          DIGITAL_PIN_17
#define NES_DATA1          DIGITAL_PIN_14

#define GC_PIN             DIGITAL_PIN_14

#define ThreeDO_LATCH      DIGITAL_PIN_17
#define ThreeDO_DATA       DIGITAL_PIN_15
#define ThreeDO_CLOCK      DIGITAL_PIN_16

#define PCFX_LATCH         DIGITAL_PIN_16
#define PCFX_CLOCK         DIGITAL_PIN_15
#define PCFX_DATA          DIGITAL_PIN_14

#define SS_SELECT0         DIGITAL_PIN_20
#define SS_SEL             DIGITAL_PIN_20
#define SS_SELECT1         DIGITAL_PIN_21
#define SS_REQ             DIGITAL_PIN_21
#define SS_ACK             DIGITAL_PIN_17
#define SS_DATA0           DIGITAL_PIN_17
#define SS_DATA1           DIGITAL_PIN_16
#define SS_DATA2           DIGITAL_PIN_15
#define SS_DATA3           DIGITAL_PIN_14

#define TG_SELECT          DIGITAL_PIN_20
#define TG_DATA1           DIGITAL_PIN_17
#define TG_DATA2           DIGITAL_PIN_16
#define TG_DATA3           DIGITAL_PIN_15
#define TG_DATA4           DIGITAL_PIN_14

#define PS_ATT             DIGITAL_PIN_17
#define PS_CLOCK           DIGITAL_PIN_16
#define PS_ACK             DIGITAL_PIN_15
#define PS_CMD             DIGITAL_PIN_14
#define PS_DATA            DIGITAL_PIN_20

//PORTD
#define NEOGEO_SELECT      DIGITAL_PIN_17
#define NEOGEO_D           DIGITAL_PIN_16
#define NEOGEO_B           DIGITAL_PIN_15
#define NEOGEO_RIGHT       DIGITAL_PIN_14
#define NEOGEO_DOWN        DIGITAL_PIN_20
#define NEOGEO_START       DIGITAL_PIN_21
//PORTB
#define NEOGEO_C           DIGITAL_PIN_17
#define NEOGEO_A           DIGITAL_PIN_07
#define NEOGEO_LEFT        DIGITAL_PIN_18
#define NEOGEO_UP          DIGITAL_PIN_19

#define INTPIN1            DIGITAL_PIN_17
#define INTPIN2            DIGITAL_PIN_16
#define INTPIN3            DIGITAL_PIN_15
#define INTPIN4            DIGITAL_PIN_14
#define INTPIN5			       NOT_CONNECTED
#define INTPIN6            DIGITAL_PIN_21
#define INTPIN7            DIGITAL_PIN_18
#define INTPIN8            DIGITAL_PIN_19
#define INTPIN9            DIGITAL_PIN_17

#define AJ_COLUMN1         DIGITAL_PIN_17
#define AJ_COLUMN2         DIGITAL_PIN_07
#define AJ_COLUMN3		     DIGITAL_PIN_18
#define AJ_COLUMN4         DIGITAL_PIN_19

#define SMS_INPUT_PIN_0    17
#define SMS_INPUT_PIN_1    16
#define SMS_INPUT_PIN_2    15
#define SMS_INPUT_PIN_3    14
#define SMS_INPUT_PIN_4    20
#define SMS_INPUT_PIN_5    21

#define SMSONGEN_INPUT_PIN_0    17
#define SMSONGEN_INPUT_PIN_1    16
#define SMSONGEN_INPUT_PIN_2    15
#define SMSONGEN_INPUT_PIN_3    14
#define SMSONGEN_INPUT_PIN_4    20
#define SMSONGEN_INPUT_PIN_5    21

#define GENESIS_TH            DIGITAL_PIN_17
#define GENESIS_TR            DIGITAL_PIN_21
#define GENESIS_TL            DIGITAL_PIN_20

#define PIND_READ( pin ) ((VPORTD.IN << 2)&(1<<(pin)))
#define PINB_READ( pin ) (VPORTA.IN & (1<<(pin)))
#define PINC_READ( pin ) digitalRead(pin)

#define READ_PORTD( mask ) ((VPORTD.IN << 2) & mask)
#define READ_PORTB( mask ) (VPORTA.IN & mask)

#define MICROSECOND_NOPS "nop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\n"

#define T_DELAY( ms ) delay(0)
#define A_DELAY( ms ) delay(ms)

#define FASTRUN
