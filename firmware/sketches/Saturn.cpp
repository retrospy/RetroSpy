//
// Saturn.cpp
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

#include "Saturn.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

void SaturnSpy::loop() {
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

void SaturnSpy::updateState() {
	word pincache = 0;

	while (READ_PORTD(0b11000000) != 0b10000000) {}
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
	for (int i = 0; i < 12; ++i)  // This is trial and error'd.  
		asm volatile("nop\n"); // NOP isn't consistent enough on an optimized Pi Pico
#else
	asm volatile(MICROSECOND_NOPS)
#endif
	pincache |= READ_PORTD(0xFF);
	if ((pincache & 0b11000000) == 0b10000000) {
		ssState3 = ~pincache;
	}

	pincache = 0;
	while (READ_PORTD(0b11000000) != 0b01000000) {}
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
	for (int i = 0; i < 12; ++i)  // This is trial and error'd.  
		asm volatile("nop\n"); // NOP isn't consistent enough on an optimized Pi Pico
#else
	asm volatile(MICROSECOND_NOPS)
#endif
	pincache |= READ_PORTD(0xFF);
	if ((pincache & 0b11000000) == 0b01000000) {
		ssState2 = ~pincache;
	}

	pincache = 0;
	while (READ_PORTD(0b11000000) != 0) {}
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
	for (int i = 0; i < 12; ++i)  // This is trial and error'd.  
		asm volatile("nop\n"); // NOP isn't consistent enough on an optimized Pi Pico
#else
	asm volatile(MICROSECOND_NOPS)
#endif
	pincache |= READ_PORTD(0xFF);
	if ((pincache & 0b11000000) == 0) {
		ssState1 = ~pincache;
	}

	pincache = 0;
	while (READ_PORTD(0b11000000) != 0b11000000) {}
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
	for (int i = 0; i < 12; ++i)  // This is trial and error'd.  
		asm volatile("nop\n"); // NOP isn't consistent enough on an optimized Pi Pico
#else
	asm volatile(MICROSECOND_NOPS)
#endif
	pincache |= READ_PORTD(0xFF);
	if ((pincache & 0b11000000) == 0b11000000) {
		ssState4 = ~pincache;
	}
}

void SaturnSpy::writeSerial() {
	for (int i = 0; i < 8; ++i) {
		Serial.write(i == 6 ? ONE : ZERO);
	}
	Serial.write((ssState3 & 0b00100000) ? ZERO : ONE);
	Serial.write((ssState3 & 0b00010000) ? ZERO : ONE);
	Serial.write((ssState3 & 0b00001000) ? ZERO : ONE);
	Serial.write((ssState3 & 0b00000100) ? ZERO : ONE);

	Serial.write((ssState2 & 0b00100000) ? ZERO : ONE);
	Serial.write((ssState2 & 0b00010000) ? ZERO : ONE);
	Serial.write((ssState2 & 0b00001000) ? ZERO : ONE);
	Serial.write((ssState2 & 0b00000100) ? ZERO : ONE);

	Serial.write((ssState1 & 0b00100000) ? ZERO : ONE);
	Serial.write((ssState1 & 0b00010000) ? ZERO : ONE);
	Serial.write((ssState1 & 0b00001000) ? ZERO : ONE);
	Serial.write((ssState1 & 0b00000100) ? ZERO : ONE);

	Serial.write((ssState4 & 0b00100000) ? ZERO : ONE);
	Serial.write(ONE);
	Serial.write(ONE);
	Serial.write(ONE);

	for (int i = 0; i < 32; ++i) {
		Serial.write(ZERO);
	}

	Serial.write(SPLIT);
}

void SaturnSpy::debugSerial() {
	Serial.print((ssState1 & 0b00000100) ? "Z" : "0");
	Serial.print((ssState1 & 0b00001000) ? "Y" : "0");
	Serial.print((ssState1 & 0b00010000) ? "X" : "0");
	Serial.print((ssState1 & 0b00100000) ? "R" : "0");

	Serial.print((ssState2 & 0b00000100) ? "B" : "0");
	Serial.print((ssState2 & 0b00001000) ? "C" : "0");
	Serial.print((ssState2 & 0b00010000) ? "A" : "0");
	Serial.print((ssState2 & 0b00100000) ? "S" : "0");

	Serial.print((ssState3 & 0b00000100) ? "u" : "0");
	Serial.print((ssState3 & 0b00001000) ? "d" : "0");
	Serial.print((ssState3 & 0b00010000) ? "l" : "0");
	Serial.print((ssState3 & 0b00100000) ? "r" : "0");

	Serial.print((ssState4 & 0b00100000) ? "L" : "0");

	Serial.print("\n");
}
#else

void SaturnSpy::loop() {}

void SaturnSpy::writeSerial() {}

void SaturnSpy::debugSerial() {}

void SaturnSpy::updateState() {}

#endif

const char* SaturnSpy::startupMsg()
{
	return "Saturn";
}