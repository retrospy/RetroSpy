//
// PlayStation.cpp
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

#include "PlayStation.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E)  || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

void PlayStationSpy::loop() {
	noInterrupts();
	updateState();
	interrupts();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
	T_DELAY(5);
}

void PlayStationSpy::updateState() {
	byte numBits = 0;
	WAIT_FALLING_EDGE(PS_ATT);

	unsigned char bits = 0;
	do {
		WAIT_LEADING_EDGE(PS_CLOCK);
	} while (++bits < 8);

	bits = 0;
	do {
		WAIT_LEADING_EDGE(PS_CLOCK);
		byte pins = READ_PORTD(0b01100000);

		rawData[numBits] = pins & 0b01000000;
		playstationCommand[numBits] = pins & 0b00100000;
		numBits++;
	} while (++bits < 8);

	bits = 0;
	do {
		WAIT_LEADING_EDGE(PS_CLOCK);
	} while (++bits < 8);

	bits = 0;
	do {
		WAIT_LEADING_EDGE(PS_CLOCK);
		
		byte pins = READ_PORTD(0b01100000);

		rawData[numBits++] = !(pins & 0b01000000);
		rawData[152 + bits] = pins & 0b00100000;
		
	} while (++bits < 16);

	//Read analog sticks for Analog Controller in Red Mode
	if((rawData[0] != 0 && rawData[1] != 0 && rawData[2] == 0 && rawData[3] == 0 && rawData[4] != 0 && rawData[5] != 0 && rawData[6] != 0 && rawData[7] == 0)    // controllerType == 0x73 (DualShock 1)
	   || (rawData[0] != 0 && rawData[1] != 0 && rawData[2] == 0 && rawData[3] == 0 && rawData[4] == 0 && rawData[5] != 0 && rawData[6] == 0 && rawData[7] == 0) // controllerType == 0x23 (NegCon)
	   || (rawData[0] != 0 && rawData[1] != 0 && rawData[2] == 0 && rawData[3] == 0 && rawData[4] != 0 && rawData[5] == 0 && rawData[6] != 0 && rawData[7] == 0) // controllerType == 0x53 (Analogue Controller)
		) 
	{
		for (int i = 0; i < 4; ++i)
		{
			bits = 0;
			do {
				WAIT_LEADING_EDGE(PS_CLOCK);
				rawData[numBits++] = PIN_READ(PS_DATA);
			} while (++bits < 8);
		}
	}
	else if (rawData[0] == 0 && rawData[1] != 0 && rawData[2] == 0 && rawData[3] == 0 && rawData[4] != 0 && rawData[5] == 0 && rawData[6] == 0 && rawData[7] == 0) {
		// controllerType == 0x12 (mouse)
		for (int i = 0; i < 2; ++i) {
			bits = 0;
			do {
				WAIT_LEADING_EDGE(PS_CLOCK);
				rawData[numBits++] = PIN_READ(PS_DATA);
			} while (++bits < 8);
		}
	}
	else if (rawData[0] != 0 && rawData[1] == 0 && rawData[2] == 0 && rawData[3] != 0 && rawData[4] != 0 && rawData[5] != 0 && rawData[6] != 0 && rawData[7] == 0) {
		// controllerType == 0x79 (DualShock 2)
		for (int i = 0; i < 16; ++i) {
			bits = 0;
			do {
				WAIT_LEADING_EDGE(PS_CLOCK);
				rawData[numBits++] = PIN_READ(PS_DATA);
			} while (++bits < 8);
		}
	}
}

void PlayStationSpy::writeSerial() {
	if (playstationCommand[0] == 0 && playstationCommand[1] != 0 && playstationCommand[2] == 0 && playstationCommand[3] == 0 && playstationCommand[4] == 0 && playstationCommand[5] == 0 && playstationCommand[6] != 0 && playstationCommand[7] == 0) {
		// playstationCommand=0x42 (Controller Poll)
		for (unsigned char i = 0; i < 168; ++i) {
			Serial.write(rawData[i] ? ONE : ZERO);
		}
		Serial.write(SPLIT);
	}
}

void PlayStationSpy::debugSerial() {
	for (int i = 0; i < 152; ++i) {
		if (i % 8 == 0) {
			Serial.print("|");
		}
		Serial.print(rawData[i] ? "1" : "0");
	}
	Serial.print("\n");
}

#else
void PlayStationSpy::loop() {}
void PlayStationSpy::debugSerial() {}
void PlayStationSpy::updateState() {}
void PlayStationSpy::writeSerial() {}
#endif

const char* PlayStationSpy::startupMsg()
{
	return "PlayStation";
}