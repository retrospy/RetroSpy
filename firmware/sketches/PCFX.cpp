//
// PCFX.cpp
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

#include "PCFX.h"

#if !(defined(__arm__) && defined(CORE_TEENSY))

void PCFXSpy::loop() {
	noInterrupts();
	updateState();
	interrupts();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
}

void PCFXSpy::updateState() {
	unsigned char bits = PCFX_BITCOUNT;
	unsigned char *rawDataPtr = rawData;

	WAIT_FALLING_EDGE(PCFX_LATCH);

	do {
		WAIT_FALLING_EDGE(PCFX_CLOCK);
		*rawDataPtr = !PIN_READ(PCFX_DATA);
		++rawDataPtr;
	} while (--bits > 0);
}

void PCFXSpy::writeSerial() {
	sendRawData(rawData, 0, PCFX_BITCOUNT);
}

void PCFXSpy::debugSerial() {
	sendRawDataDebug(rawData, 0, PCFX_BITCOUNT);
}
#else

void PCFXSpy::loop() {}

void PCFXSpy::writeSerial() {}

void PCFXSpy::debugSerial() {}

void PCFXSpy::updateState() {}

#endif

const char* PCFXSpy::startupMsg()
{
	return "PC-FX";
}