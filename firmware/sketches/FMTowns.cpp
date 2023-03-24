//
// FMTowns.cpp
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

#include "FMTowns.h"

#if !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)

void FMTownsSpy::loop() {
	noInterrupts();
	updateState();
	interrupts();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
}

void FMTownsSpy::updateState() {
	rawData[0] = PIN_READ(2);
	rawData[1] = PIN_READ(3);
	rawData[2] = PIN_READ(4);
	rawData[3] = PIN_READ(5);
	rawData[4] = PIN_READ(6);
	rawData[5] = PIN_READ(7);
	rawData[6] = PINB_READ(0);
	rawData[7] = PINB_READ(1);
	rawData[8] = PINB_READ(2);
}

void FMTownsSpy::writeSerial() {
	for (unsigned char i = 0; i < 9; ++i) {
		Serial.write(rawData[i] ? ZERO : ONE);
	}
	Serial.write(SPLIT);
}

void FMTownsSpy::debugSerial() {
	for (int i = 0; i < 9; ++i) {
		Serial.print(rawData[i] ? "0" : "1");
	}
	Serial.print("\n");
}

#else

void FMTownsSpy::loop() {}

void FMTownsSpy::writeSerial() {}

void FMTownsSpy::debugSerial() {}

void FMTownsSpy::updateState() {}

#endif

const char* FMTownsSpy::startupMsg()
{
	return "FM Towns";
}