//
// PowerGlove.cpp
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

#include "PowerGlove.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) ||  defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

static bool sync = false;

void PowerGloveSpy::loop() {

	updateState();

#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
	T_DELAY(5);
}

void PowerGloveSpy::writeSerial() {
	sendRawData(rawData, 0, NES_BITCOUNT * 10);
}

void PowerGloveSpy::debugSerial() {
	//sendRawDataDebug(rawData, 0, NES_BITCOUNT * 10);
	
	for(int i = 0 ; i < 5 ; ++i)
	{
		int b = 0;
		for (int j = 0; j < 8; ++j)
		{
			b |= rawData[i * 8 + j] == 0 ? 0b00000000 : (1 << (7-j));
			
		}
		Serial.print(b, HEX);
		Serial.print("|");
	}
	
	for (int i = 0; i < 8; ++i)
	{
		Serial.print(rawData[40 + i] == 0 ? 0 : 1);
	}
	Serial.print("|");
	
	for (int i = 0; i < 8; ++i)
	{
		Serial.print(rawData[48 + i] == 0 ? 0 : 1);
	}
	
	Serial.print("\n");
}

void PowerGloveSpy::updateState() {

	unsigned char bits = NES_BITCOUNT*10;
	unsigned char *rawDataPtr = rawData;

	if (!sync)
	{
		noInterrupts();
		WAIT_FALLING_EDGE(NES_LATCH);
		interrupts();
		delay(5);
	}
	
	while (true)
	{
		noInterrupts();
		WAIT_FALLING_EDGE(NES_LATCH);
		int i = 0;
		do {
			WAIT_FALLING_EDGE(NES_CLOCK);
			*rawDataPtr = PIN_READ(NES_DATA);
			++rawDataPtr;
		} while (++i < 8);
		interrupts();
		
		if (rawData[0] != 0 && rawData[1] == 0 && rawData[2] != 0 && rawData[3] == 0 &&rawData[4] == 0 &&rawData[5] == 0 &&rawData[6] == 0 &&rawData[7] == 0)
		{
			bits -= 8;
			sync = true;
			break;
		}
		else
		{
			rawDataPtr = rawData;
		}
	}
	
	noInterrupts();
	WAIT_FALLING_EDGE(NES_LATCH);

	do {
		WAIT_FALLING_EDGE(NES_CLOCK);
		*rawDataPtr = PIN_READ(NES_DATA);
		++rawDataPtr;
	} while (--bits > 0);
	
	interrupts();
}

#else

void PowerGloveSpy::loop() {}

void PowerGloveSpy::writeSerial() {}

void PowerGloveSpy::debugSerial() {}

void PowerGloveSpy::updateState() {}

#endif

const char* PowerGloveSpy::startupMsg()
{
	return "Power Glove";
}