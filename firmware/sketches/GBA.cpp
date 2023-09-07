//
// GBA.cpp
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

#include "GBA.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

void GBASpy::loop() {
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

void GBASpy::writeSerial() {
	sendRawData(rawData, 0, bytesToReturn);
}

void GBASpy::debugSerial() {
	sendRawDataDebug(rawData, 0, bytesToReturn);
}

void GBASpy::updateState() {

	unsigned char position = 0;
	unsigned char bits = 0;

	bytesToReturn = SNES_BITCOUNT;

	WAIT_FALLING_EDGE(SNES_LATCH);
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
	for (int i = 0; i < 12; ++i)  // This is trial and error'd.  
		asm volatile("nop\n"); // NOP isn't consistent enough on an optimized Pi Pico
#else
	asm volatile(MICROSECOND_NOPS)
#endif
	rawData[position++] = !PIN_READ(SNES_DATA);
	do {
		WAIT_FALLING_EDGE(SNES_CLOCK);
		rawData[position++] = !PIN_READ(SNES_DATA);
	} while (++bits < SNES_BITCOUNT - 1);
}

const char* GBASpy::startupMsg()
{
	return "GBA Consolizer";
}

#else
void GBASpy::loop() {}

void GBASpy::writeSerial() {}

void GBASpy::debugSerial() {}

void GBASpy::updateState() {}

const char* GBASpy::startupMsg()
{
	return "Mode not compatible with this hardware";
}

#endif

