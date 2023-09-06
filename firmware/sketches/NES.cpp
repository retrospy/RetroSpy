//
// NES.cpp
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

#include "NES.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

void NESSpy::loop() {
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

void NESSpy::writeSerial() {
	sendRawData(rawData, 0, NES_BITCOUNT * 3);
}

void NESSpy::debugSerial() {
	sendRawDataDebug(rawData, 0, NES_BITCOUNT * 3);
}

void NESSpy::updateState() {
#ifdef MODE_2WIRE_NES
	read_shiftRegister_2wire(rawData, NES_LATCH, NES_DATA, true, NES_BITCOUNT);
#else
	unsigned char bits = NES_BITCOUNT;
	unsigned char *rawDataPtr = rawData;

	WAIT_FALLING_EDGE(NES_LATCH);

	do {
		WAIT_FALLING_EDGE(NES_CLOCK);
		*rawDataPtr = !PIN_READ(NES_DATA);
		*(rawDataPtr + 8) = !PIN_READ(NES_DATA0);
		*(rawDataPtr + 16) = !PIN_READ(NES_DATA1);
		++rawDataPtr;
	} while (--bits > 0);
#endif
}

#else

void NESSpy::loop() {}

void NESSpy::writeSerial() {}

void NESSpy::debugSerial() {}

void NESSpy::updateState() {}

#endif

const char* NESSpy::startupMsg()
{
	return "NES";
}