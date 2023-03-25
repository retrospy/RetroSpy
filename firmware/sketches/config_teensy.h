//
// config_teensy.h
//
// Author:
//       Christopher "Zoggins" Mallery <zoggins@retro-spy.com>
//
// Copyright (c) 2020 RetroSpy Technologies
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

#define NOT_CONNECTED	   0

// PORT D
#define DIGITAL_PIN_02	   0
#define DIGITAL_PIN_14	   1
#define DIGITAL_PIN_07	   2
#define DIGITAL_PIN_08	   3
#define DIGITAL_PIN_06	   4
#define DIGITAL_PIN_20	   5
#define DIGITAL_PIN_21	   6
#define DIGITAL_PIN_05	   7
// PORT B
#define DIGITAL_PIN_16	   0
#define DIGITAL_PIN_27	   1
#define DIGITAL_PIN_19	   2
#define DIGITAL_PIN_18	   3

#define MODEPIN_SNES       33
#define MODEPIN_N64        34
#define MODEPIN_GC         35
#define MODEPIN_DREAMCAST  36
#define MODEPIN_WII        37

#define N64_PIN	           DIGITAL_PIN_07

#define SNES_LATCH         DIGITAL_PIN_08
#define SNES_DATA          DIGITAL_PIN_06
#define SNES_CLOCK         DIGITAL_PIN_21

#define NES_LATCH          DIGITAL_PIN_08
#define NES_CLOCK          DIGITAL_PIN_21
#define NES_DATA           DIGITAL_PIN_06
#define NES_DATA0          DIGITAL_PIN_07
#define NES_DATA1          DIGITAL_PIN_20

#define GC_PIN             DIGITAL_PIN_20

#define ThreeDO_LATCH      NOT_CONNECTED
#define ThreeDO_DATA       NOT_CONNECTED
#define ThreeDO_CLOCK      NOT_CONNECTED

#define PCFX_LATCH         NOT_CONNECTED
#define PCFX_CLOCK         NOT_CONNECTED
#define PCFX_DATA          NOT_CONNECTED

#define SS_SELECT0         DIGITAL_PIN_21
#define SS_SEL             DIGITAL_PIN_21
#define SS_SELECT1         DIGITAL_PIN_05
#define SS_REQ             DIGITAL_PIN_05
#define SS_ACK             DIGITAL_PIN_16
#define SS_DATA0           DIGITAL_PIN_07
#define SS_DATA1           DIGITAL_PIN_08
#define SS_DATA2           DIGITAL_PIN_06
#define SS_DATA3           DIGITAL_PIN_20

#define TG_SELECT          NOT_CONNECTED
#define TG_DATA1           NOT_CONNECTED
#define TG_DATA2           NOT_CONNECTED
#define TG_DATA3           NOT_CONNECTED
#define TG_DATA4           NOT_CONNECTED

#define PS_ATT             DIGITAL_PIN_07
#define PS_CLOCK           DIGITAL_PIN_08
#define PS_ACK             DIGITAL_PIN_06
#define PS_CMD             DIGITAL_PIN_20
#define PS_DATA            DIGITAL_PIN_21

//PORTD
#define NEOGEO_SELECT      DIGITAL_PIN_07
#define NEOGEO_D           DIGITAL_PIN_08
#define NEOGEO_B           DIGITAL_PIN_06
#define NEOGEO_RIGHT       DIGITAL_PIN_20
#define NEOGEO_DOWN        DIGITAL_PIN_21
#define NEOGEO_START       DIGITAL_PIN_05
//PORTB
#define NEOGEO_C           DIGITAL_PIN_16
#define NEOGEO_A           DIGITAL_PIN_27
#define NEOGEO_LEFT        DIGITAL_PIN_19
#define NEOGEO_UP          DIGITAL_PIN_18

#define INTPIN1            NOT_CONNECTED
#define INTPIN2            NOT_CONNECTED
#define INTPIN3            NOT_CONNECTED
#define INTPIN4            NOT_CONNECTED
#define INTPIN5			   NOT_CONNECTED
#define INTPIN6            NOT_CONNECTED
#define INTPIN7            NOT_CONNECTED
#define INTPIN8            NOT_CONNECTED
#define INTPIN9            NOT_CONNECTED

#define AJ_COLUMN1         NOT_CONNECTED
#define AJ_COLUMN2         NOT_CONNECTED
#define AJ_COLUMN3		   NOT_CONNECTED
#define AJ_COLUMN4         NOT_CONNECTED

#define CD32_LATCH    DIGITAL_PIN_20
#define CD32_DATA     DIGITAL_PIN_05
#define CD32_CLOCK    DIGITAL_PIN_21

#define SMS_INPUT_PIN_0     7
#define SMS_INPUT_PIN_1     8
#define SMS_INPUT_PIN_2     6
#define SMS_INPUT_PIN_3    20
#define SMS_INPUT_PIN_4     5
#define SMS_INPUT_PIN_5    16

#define SMSONGEN_INPUT_PIN_0     7
#define SMSONGEN_INPUT_PIN_1     8
#define SMSONGEN_INPUT_PIN_2     6
#define SMSONGEN_INPUT_PIN_3    20
#define SMSONGEN_INPUT_PIN_4    21
#define SMSONGEN_INPUT_PIN_5     5

#define GENESIS_TH            DIGITAL_PIN_16
#define GENESIS_TR            DIGITAL_PIN_05
#define GENESIS_TL            DIGITAL_PIN_21

#define WII_SCL					19
#define WII_SDA				    18
#define WII_BIT_SCL			   DIGITAL_PIN_19
#define WII_BIT_SDA			   DIGITAL_PIN_18

#define READ_PORTD( mask ) (GPIOD_PDIR & mask)
#define READ_PORTB( mask ) (GPIOB_PDIR & mask)

#define PIND_READ( pin )  ((READ_PORTD(0xFF))&(1<<(pin)))
#define PINB_READ( pin )  ((READ_PORTB(0xF))&(1<<(pin)))
#define PINC_READ( pin )  (digitalReadFast(pin))

#define MICROSECOND_NOPS "nop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\nnop\n"

#define T_DELAY( ms ) delay(ms)
#define A_DELAY( ms ) delay(0)
