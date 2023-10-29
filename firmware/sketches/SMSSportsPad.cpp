//
// SMSSportsPad.cpp
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

#include "SMSSportsPad.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

static bool sync = false;

static byte x, y;
static bool button1, button2;

void SMSSportsPadSpy::setup()
{
	// Setup input pins
	for(byte i = 2 ; i <= 8 ; i++)
	{
		pinMode(i, INPUT_PULLUP);
	}
	
}

void SMSSportsPadSpy::loop()
{
	updateState();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
	
	delay(5);
}

void SMSSportsPadSpy::writeSerial()
{	
	Serial.write(x == 10 ? 11 : x);
	Serial.write(y == 10 ? 11 : y);
	Serial.write(button1 == true ? ONE : ZERO);
	Serial.write(button2 == true ? ONE : ZERO);
	Serial.write(SPLIT);
}

void SMSSportsPadSpy::debugSerial()
{
	Serial.print(x);
	Serial.print("|");
	Serial.print(y);
	Serial.print("|");
	Serial.print(button1 == true ? "1" : "0");
	Serial.println(button2 == true ? "2" : "0");
}

void SMSSportsPadSpy::updateState()
{
	x = y = 0;
	if (!sync)
	{
		noInterrupts();
		WAIT_FALLING_EDGEB(0);
		interrupts();
		delay(5);
		sync = true;
	}
	
	noInterrupts();
	WAIT_FALLING_EDGEB(0);
	y |= (READ_PORTD(0b00111100)) >> 2;
	WAIT_LEADING_EDGEB(0);
	x |= (READ_PORTD(0b00111100)) << 2;
	WAIT_FALLING_EDGEB(0);
	x |= (READ_PORTD(0b00111100)) >> 2;
	WAIT_LEADING_EDGEB(0);
	y |= (READ_PORTD(0b00111100)) << 2;
	
	button1 = PIN_READ(6) == 0;
	button2 = PIN_READ(7) == 0;
	interrupts();
}

#else
void SMSSportsPadSpy::setup() {}

void SMSSportsPadSpy::loop() {}

void SMSSportsPadSpy::writeSerial() {}

void SMSSportsPadSpy::debugSerial() {}

void SMSSportsPadSpy::updateState() {}

#endif

const char* SMSSportsPadSpy::startupMsg()
{
	return "SMS Sports Pad";
}