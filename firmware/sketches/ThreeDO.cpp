//
// ThreeDO.cpp
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

#include "ThreeDO.h"

#if !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)

void ThreeDOSpy::setup(uint8_t cableType) {
	this->cableType = cableType;
	setup();
}

void ThreeDOSpy::setup() {

	PORTD = 0x00;
	PORTB = 0x00;
	DDRD = 0x00;

	for (int i = 2; i <= 6; ++i)
		pinMode(i, INPUT_PULLUP);
}

void ThreeDOSpy::loop() {
	noInterrupts();
	updateState();
	interrupts();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
}

void ThreeDOSpy::updateState()
{
	if (cableType == CABLE_SMS)
		return updateStateLegacy();
	
	return updateStateVision();
}

void ThreeDOSpy::updateStateLegacy() {
	unsigned char *rawDataPtr = rawData;

	unsigned char bits = 0;
	WAIT_FALLING_EDGE(ThreeDO_LATCH);

	do {
		WAIT_LEADING_EDGE(ThreeDO_CLOCK);
		*rawDataPtr = PIN_READ(ThreeDO_DATA);

		if (bits == 0 && *rawDataPtr != 0)
			bytesToReturn = bits = 32;
		else if (bits == 0)
			bytesToReturn = bits = 16;

		++rawDataPtr;
	} while (--bits > 0);
}

void ThreeDOSpy::updateStateVision() {
	unsigned char *rawDataPtr = rawData;

	unsigned char bits = 0;
	WAIT_FALLING_EDGE(VIS_ThreeDO_LATCH);

	do {
		WAIT_LEADING_EDGEB(VIS_ThreeDO_CLOCK);
		*rawDataPtr = PIN_READ(VIS_ThreeDO_DATA);

		if (bits == 0 && *rawDataPtr != 0)
			bytesToReturn = bits = 32;
		else if (bits == 0)
			bytesToReturn = bits = 16;

		++rawDataPtr;
	} while (--bits > 0);
}

void ThreeDOSpy::writeSerial() {
	sendRawData(rawData, 0, bytesToReturn);
}

void ThreeDOSpy::debugSerial() {
	sendRawDataDebug(rawData, 0, bytesToReturn);
}

#else
void ThreeDOSpy::loop() {}

void ThreeDOSpy::writeSerial() {}

void ThreeDOSpy::debugSerial() {}

void ThreeDOSpy::updateState() {}

#endif

const char* ThreeDOSpy::startupMsg()
{
	return "3DO";
}