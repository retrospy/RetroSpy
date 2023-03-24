//
// TG16.cpp
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

#include "TG16.h"

#if !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)

void TG16Spy::loop() {

	noInterrupts();
	updateState();
	interrupts();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
	delay(1);
}
static bool has6buttons = false;
static int roundsSince6Button = 0;
void TG16Spy::updateState() {
	word temp = 0;
	currentState = 0;

	while ((READ_PORTD(0b01000000)) == 0) {}
	asm volatile("nop\nnop\n");
	temp = ((READ_PORTD(0b00111100)) >> 2);
	if ((temp & 0b00001111) == 0b00000000) {
		currentState |= lastDirections;
		seenHighButtons = true;
		has6buttons = true;
		roundsSince6Button = 0;
	}
	else {
		
		if (++roundsSince6Button == 2)
		{
			has6buttons = false;
			lastHighButtons = 0x00F0;
		}
		
		seenHighButtons = false;
		currentState |= temp;
		lastDirections = temp;
	}

	while ((READ_PORTD(0b01000000)) != 0) {}
	asm volatile(MICROSECOND_NOPS);
	temp = ((READ_PORTD(0b00111100)) << 2);
	if (seenHighButtons == has6buttons ? false : true) {
		currentState |= (temp << 4);
		lastHighButtons = temp;
		currentState |= lastButtons;
	}
	else {
		currentState |= temp;
		lastButtons = temp;
		currentState |= (lastHighButtons << 4);
	}

	currentState = ~currentState;
}

void TG16Spy::writeSerial() {
	for (unsigned char i = 0; i < 12; ++i) {
		Serial.write(currentState & (1 << i) ? ONE : ZERO);
	}
	Serial.write(SPLIT);
}

void TG16Spy::debugSerial() {
	Serial.print((currentState & 0b0000000000000001) ? "U" : "0");
	Serial.print((currentState & 0b0000000000000010) ? "R" : "0");
	Serial.print((currentState & 0b0000000000000100) ? "D" : "0");
	Serial.print((currentState & 0b0000000000001000) ? "L" : "0");
	Serial.print((currentState & 0b0000000000010000) ? "A" : "0");
	Serial.print((currentState & 0b0000000000100000) ? "B" : "0");
	Serial.print((currentState & 0b0000000001000000) ? "S" : "0");
	Serial.print((currentState & 0b0000000010000000) ? "R" : "0");
	Serial.print((currentState & 0b0000000100000000) ? "3" : "0");
	Serial.print((currentState & 0b0000001000000000) ? "4" : "0");
	Serial.print((currentState & 0b0000010000000000) ? "5" : "0");
	Serial.print((currentState & 0b0000100000000000) ? "6" : "0");
	Serial.print("\n");
}

#else
void TG16Spy::loop() {}

void TG16Spy::writeSerial() {}

void TG16Spy::debugSerial() {}

void TG16Spy::updateState() {}

#endif

const char* TG16Spy::startupMsg()
{
	return "TG16";
}