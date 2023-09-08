//
// NeoGeo.cpp
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

#include "NeoGeo.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E)  || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

void NeoGeoSpy::loop() {
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

void NeoGeoSpy::updateState() {
	rawData[0] = PIN_READ(NEOGEO_SELECT);
	rawData[1] = PIN_READ(NEOGEO_D);
	rawData[2] = PIN_READ(NEOGEO_B);
	rawData[3] = PIN_READ(NEOGEO_RIGHT);
	rawData[4] = PIN_READ(NEOGEO_DOWN);
	rawData[5] = PIN_READ(NEOGEO_START);
	rawData[6] = PINB_READ(NEOGEO_C);
	rawData[7] = PINB_READ(NEOGEO_A);
	rawData[8] = PINB_READ(NEOGEO_LEFT);
	rawData[9] = PINB_READ(NEOGEO_UP);
}

void NeoGeoSpy::writeSerial() {
	for (unsigned char i = 0; i < 10; ++i) {
		Serial.write(rawData[i] ? ZERO : ONE);
	}
	Serial.write(SPLIT);
}

void NeoGeoSpy::debugSerial() {
	for (int i = 0; i < 10; ++i) {
		Serial.print(rawData[i] ? "0" : "1");
	}
	Serial.print("\n");
}

#else

void NeoGeoSpy::loop() {}

void NeoGeoSpy::writeSerial() {}

void NeoGeoSpy::debugSerial() {}

void NeoGeoSpy::updateState() {}

#endif

const char* NeoGeoSpy::startupMsg()
{
	return "Neo Geo";
}