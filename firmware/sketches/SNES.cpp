//
// SNES.cpp
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

#include "SNES.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

void SNESSpy::setup1() {
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
	// Disable the built-in pull-up and pull-down resistors and input the signals divided by 10kƒ¶ and 20kƒ¶ external resistors.
	// If the resistance value used for voltage division is too low,
	// SNES and RetroSpy will recognize that all buttons are being pressed when no controller is connected.
	pinMode(SNES_LATCH, INPUT);
	pinMode(SNES_DATA,  INPUT);
	pinMode(SNES_CLOCK, INPUT);
#endif
}

void SNESSpy::loop() {
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	loop1();
#endif

	if (sendRequest)
	{
		sendBytes = bytesToReturn;
		memcpy(sendData, rawData, SNES_BITCOUNT_EXT);
		sendRequest = false;
#ifdef DEBUG
		debugSerial();
#else
		writeSerial();
#endif
		T_DELAY(5);
	}
}

void SNESSpy::loop1() {
	while (sendRequest)
	{
	}
	updateState();
	sendRequest = true;
}

void SNESSpy::writeSerial() {
	sendRawData(sendData, 0, sendBytes);
}

void SNESSpy::debugSerial() {
	sendRawDataDebug(sendData, 0, sendBytes);
}

void SNESSpy::updateState() {
#ifdef MODE_2WIRE_SNES
	read_shiftRegister_2wire(rawData, SNES_LATCH, SNES_DATA, false, SNES_BITCOUNT);
#else
	unsigned char position = 0;
	unsigned char bits = 0;
	unsigned long start;
	bytesToReturn = SNES_BITCOUNT;

waiting_for_latch:
	start = millis();
	position = 0;
	bits = 0;
	WAIT_FALLING_EDGE(SNES_LATCH);
	if (millis() - start < 10)
	{
		goto waiting_for_latch;
	}
	
	noInterrupts();
	do {
		WAIT_FALLING_EDGE(SNES_CLOCK);
		rawData[position++] = !PIN_READ(SNES_DATA);
	} while (++bits < SNES_BITCOUNT);

	if (rawData[15] != 0 && rawData[0] != 0)
	{
		interrupts();
		goto waiting_for_latch;
	}
	
	if (rawData[15] != 0x0 || (rawData[15] == 0x00 && rawData[13] != 0x00))
	{
		bits = 0;
		do {
			WAIT_FALLING_EDGE(SNES_CLOCK);
			rawData[position++] = !PIN_READ(SNES_DATA);
		} while (++bits < SNES_BITCOUNT);

		bytesToReturn = SNES_BITCOUNT_EXT;
	}
	interrupts();
#endif
}

#else
void SNESSpy::loop() {}

void SNESSpy::writeSerial() {}

void SNESSpy::debugSerial() {}

void SNESSpy::updateState() {}

#endif

const char* SNESSpy::startupMsg()
{
	return "SNES";
}