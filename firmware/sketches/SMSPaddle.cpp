//
// SMSPaddle.cpp
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

#include "SMSPaddle.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

static int value;
static bool button;

void SMSPaddleSpy::setup(uint8_t cableType) {
	this->cableType = cableType;
	setup();
}

void SMSPaddleSpy::setup()
{
	// Setup input pins
	for(byte i = 2 ; i <= 8 ; i++)
	{
		pinMode(i, INPUT_PULLUP);
	}
	
}

void SMSPaddleSpy::loop()
{
	updateState();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
}

void SMSPaddleSpy::writeSerial()
{
	Serial.write(value == 10 ? 11 : value);
	Serial.write(button == true ? ONE : ZERO);
	Serial.write(SPLIT);
}

void SMSPaddleSpy::debugSerial()
{
	Serial.print(value);
	Serial.print("|");
	Serial.println(button == true ? "B" : "0");
}

void SMSPaddleSpy::updateState()
{
	if (cableType == CABLE_SMS)
		return updateStateLegacy();
	
	return updateStateVision();
}

void SMSPaddleSpy::updateStateLegacy()
{
	noInterrupts();
	value = 0;
	WAIT_FALLING_EDGEB(0);
	value |= (READ_PORTD(0b00111100)) >> 2;
	WAIT_LEADING_EDGEB(0);
	value |= (READ_PORTD(0b00111100)) << 2;
	button = PIN_READ(7) == 0;
	interrupts();
}

void SMSPaddleSpy::updateStateVision()
{
	noInterrupts();
	value = 0;
	WAIT_FALLING_EDGE(7);
	value |= (READ_PORTD(0b00111100)) >> 2;
	WAIT_LEADING_EDGE(7);
	value |= (READ_PORTD(0b00111100)) << 2;
	button = PIN_READ(6) == 0;
	interrupts();
}

#else
void SMSPaddleSpy::setup() {}

void SMSPaddleSpy::loop() {}

void SMSPaddleSpy::writeSerial() {}

void SMSPaddleSpy::debugSerial() {}

void SMSPaddleSpy::updateState() {}

#endif

const char* SMSPaddleSpy::startupMsg()
{
	return "SMS Paddle";
}