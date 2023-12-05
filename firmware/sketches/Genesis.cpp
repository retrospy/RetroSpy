//
// Genesis.cpp
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

#include "Genesis.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

static unsigned long settleStart;
static unsigned long waitStart;

void GenesisSpy::setup() {
#if defined(__arm__) && defined(CORE_TEENSY)
	// GPIOD_PDIR & 0xFF;
	pinMode(2, INPUT_PULLUP);
	pinMode(14, INPUT_PULLUP);
	pinMode(7, INPUT_PULLUP);
	pinMode(8, INPUT_PULLUP);
	pinMode(6, INPUT_PULLUP);
	pinMode(20, INPUT_PULLUP);
	pinMode(21, INPUT_PULLUP);
	pinMode(5, INPUT_PULLUP);

	// GPIOB_PDIR & 0xF;
	pinMode(16, INPUT_PULLUP);
	pinMode(17, INPUT_PULLUP);
	pinMode(19, INPUT_PULLUP);
	pinMode(18, INPUT_PULLUP);
#elif defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
	for (byte i = 0; i <= 6; i++)
	{
		pinMode(i, INPUT_PULLUP);
	}
#else
	// Setup input pins
	// Assumes pin 8 is SELECT (DB9 pin 7)
	// Assumes pins 2-7 are DB9 pins 1,2,3,4,6,9
	// DB9 pin 5 is +5V !!!DO NOT CONNECT TO THE ARDUINO!!!!
	// DB9 pin 8 is ground (connecting probably won't hurt, but its unnecessary to connect it to the Arduino)
	for (byte i = 2; i <= 8; i++)
	{
		pinMode(i, INPUT_PULLUP);
	}
#endif
}

void GenesisSpy::loop() {
	updateState();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
	T_DELAY(5);
	A_DELAY(1);
}

void GenesisSpy::updateState() {
	currentState = 0xFFFF;

	noInterrupts();

	do {
		WORKING_WAIT
	} while (WAIT_FOR_STATE_TWO);
	WAIT_FOR_LINES_TO_SETTLE;
	currentState &= SHIFT_A_AND_START;

	do {
		WORKING_WAIT
	} while (WAIT_FOR_STATE_THREE);
	WAIT_FOR_LINES_TO_SETTLE;
	currentState &= SHIFT_UDLRBC;

	// Six Button
	do {
		WORKING_WAIT
	} while (WAIT_FOR_STATE_FOUR_OR_SIX);

	if (NOT_STATE_SIX)
	{
		//currentState &= SHIFT_A_AND_START;
		do {
			WORKING_WAIT
		} while (WAIT_FOR_STATE_THREE);
		//currentState &= SHIFT_UDLRBC;

		do {
			WORKING_WAIT
		} while (WAIT_FOR_STATE_FOUR_OR_SIX);
	}

	if (STATE_SIX)
	{
		do {
			WORKING_WAIT
		} while (WAIT_FOR_STATE_SEVEN);
		WAIT_FOR_LINES_TO_SETTLE;
		currentState &= SHIFT_ZYXM;
	}

	interrupts();

	currentState = ~currentState;
}

void GenesisSpy::writeSerial() {
	for (unsigned char i = 0; i < 13; ++i)
	{
		Serial.write(currentState & (1 << i) ? ONE : ZERO);
	}
	Serial.write(SPLIT);
}

void GenesisSpy::debugSerial() {
	if (currentState != lastState)
	{
		Serial.print((currentState & SCS_CTL_ON) ? "+" : "-");
		Serial.print((currentState & SCS_BTN_UP) ? "U" : "0");
		Serial.print((currentState & SCS_BTN_DOWN) ? "D" : "0");
		Serial.print((currentState & SCS_BTN_LEFT) ? "L" : "0");
		Serial.print((currentState & SCS_BTN_RIGHT) ? "R" : "0");
		Serial.print((currentState & SCS_BTN_START) ? "S" : "0");
		Serial.print((currentState & SCS_BTN_A) ? "A" : "0");
		Serial.print((currentState & SCS_BTN_B) ? "B" : "0");
		Serial.print((currentState & SCS_BTN_C) ? "C" : "0");
		Serial.print((currentState & SCS_BTN_X) ? "X" : "0");
		Serial.print((currentState & SCS_BTN_Y) ? "Y" : "0");
		Serial.print((currentState & SCS_BTN_Z) ? "Z" : "0");
		Serial.print((currentState & SCS_BTN_MODE) ? "M" : "0");
		Serial.print("\n");
		lastState = currentState;
	}
}

#else
void GenesisSpy::setup() {}

void GenesisSpy::loop() {}

void GenesisSpy::writeSerial() {}

void GenesisSpy::debugSerial() {}

void GenesisSpy::updateState() {}

#endif

const char* GenesisSpy::startupMsg()
{
	return "Genesis";
}