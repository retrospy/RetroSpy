//
// Saturn3D.cpp
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

#include "Saturn3D.h"

#if defined(ARDUINO_TEENSY35) || defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

static bool isKeyboard = false;
static byte keyboardData[18];
static bool isMake = false;
static bool isBreak = false;
static byte keycode = 0;

void Saturn3DSpy::loop() {

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

void Saturn3DSpy::setup()
{
#if defined(ARDUINO_AVR_NANO_EVERY)
	// (PORTD.IN & 0x3F) << 2;
	pinMode(17, INPUT_PULLUP);
	pinMode(16, INPUT_PULLUP);
	pinMode(15, INPUT_PULLUP);
	pinMode(14, INPUT_PULLUP);
	pinMode(20, INPUT_PULLUP);
	pinMode(21, INPUT_PULLUP);

	// PORTA.IN & 0xF;
	pinMode(2, INPUT_PULLUP);
	pinMode(7, INPUT_PULLUP);
	pinMode(18, INPUT_PULLUP);
	pinMode(19, INPUT_PULLUP);
	
	PORTD.OUT = 0x00;
	PORTA.OUT = 0x00;
#elif defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)	
	for (int i = 0; i <= 8; ++i)
		pinMode(i, INPUT_PULLUP);
#else
	PORTD = 0x00;
	PORTB = 0x00;
	DDRD = 0x00;

	for (int i = 2; i <= 6; ++i)
		pinMode(i, INPUT_PULLUP);
#endif
	for (int i = 0; i < 18; ++i)
	{
		keyboardData[i] = 0;
	}
}

void Saturn3DSpy::updateState() {
	byte numBits = 0;

	isKeyboard = false;
	
	WAIT_FALLING_EDGE(SS_SEL);

	for (int i = 0; i < 3; ++i) {
		WAIT_FALLING_EDGE(SS_REQ);
		WAIT_FALLING_EDGEB(SS_ACK);

		rawData[numBits++] = PIN_READ(SS_DATA3);
		rawData[numBits++] = PIN_READ(SS_DATA2);
		rawData[numBits++] = PIN_READ(SS_DATA1);
		rawData[numBits++] = PIN_READ(SS_DATA0);

		WAIT_LEADING_EDGE(SS_REQ);
		WAIT_LEADING_EDGEB(SS_ACK);

		rawData[numBits++] = PIN_READ(SS_DATA3);
		rawData[numBits++] = PIN_READ(SS_DATA2);
		rawData[numBits++] = PIN_READ(SS_DATA1);
		rawData[numBits++] = PIN_READ(SS_DATA0);
	}

	if (rawData[3] != 0)
	{
		int numBytes = 4;  // 3D Controller

		if (rawData[2] != 0) {
			
			if (rawData[4] == 0) // Keyboard
			{
				isKeyboard = true;
				numBytes = 2;
			}
			else // Mouse
				numBytes = 1;
		}

		for (int i = 0; i < numBytes; ++i)
		{
			WAIT_FALLING_EDGE(SS_REQ);
			WAIT_FALLING_EDGEB(SS_ACK);

			rawData[numBits++] = PIN_READ(SS_DATA3);
			rawData[numBits++] = PIN_READ(SS_DATA2);
			rawData[numBits++] = PIN_READ(SS_DATA1);
			rawData[numBits++] = PIN_READ(SS_DATA0);

			WAIT_LEADING_EDGE(SS_REQ);
			WAIT_LEADING_EDGEB(SS_ACK);

			rawData[numBits++] = PIN_READ(SS_DATA3);
			rawData[numBits++] = PIN_READ(SS_DATA2);
			rawData[numBits++] = PIN_READ(SS_DATA1);
			rawData[numBits++] = PIN_READ(SS_DATA0);
		}
	}
	else {
		rawData[numBits++] = 1;
		for (int i = 0; i < 7; ++i) {
			rawData[numBits++] = 0;
		}

		rawData[numBits++] = 1;
		for (int i = 0; i < 7; ++i) {
			rawData[numBits++] = 0;
		}

		for (int i = 0; i < 16; ++i) {
			rawData[numBits++] = 0;
		}
	}
	
	isKeyboard = isKeyboard && rawData[21] == 0 && rawData[22] == 0 && rawData[23] == 0;
	if (isKeyboard)
	{
		isMake = rawData[28] != 0;
		isBreak = rawData[31] != 0;
		
		keycode = 0;

		for (int i = 7; i >= 0; i--)
		{
			if (rawData[32 + (7-i)] != 0)
			{
				keycode |= (1 << i);
			}
		}
		
		// This is a hack for the disappearing 7 bit
		if(isBreak && keycode < 16 && (keyboardData[keycode / 8] & (1 << (keycode % 8))) == 0)
		{
			keycode |= 0b10000000;
		}
		else if (isBreak && keycode < 16 && (keyboardData[keycode / 8] & (1 << (keycode % 8))) != 0
									&& (keyboardData[(keycode | 0b10000000) / 8] & (1 << ((keycode | 0b10000000) % 8))) != 0)
		{
			// both pushed, so clear the high keycode here.
			keyboardData[(keycode | 0b10000000) / 8] &= ~(1 << ((keycode | 0b10000000) % 8));
		}
		
		if (isMake)
			keyboardData[keycode / 8] |= (1 << (keycode % 8));
		
		if (isBreak)
			keyboardData[keycode/8] &= ~(1 << (keycode % 8));
	}
}

void Saturn3DSpy::writeSerial() {
	if (!isKeyboard)
	{
		for (unsigned char i = 0; i < 56; ++i)
		{
			Serial.write(rawData[i] ? ONE : ZERO);
		}
	}
	else
	{
		for (int i = 0; i < 18; ++i)
		{
			Serial.write((keyboardData[i] & 0x0F) << 4);
			Serial.write(keyboardData[i] & 0xF0);
		}
	}
	
	Serial.write(SPLIT);
}

void Saturn3DSpy::debugSerial() {
	
	if (isKeyboard)
	{
		if (isMake)
			Serial.print("1|");
		else
			Serial.print("0|");
		
		if (isBreak)
			Serial.print("1|");
		else
			Serial.print("0|");
		
		Serial.print(keycode, HEX);
		Serial.print("|");
		
		for (int i = 0; i < 18; ++i)
		{	
			Serial.print(keyboardData[i], HEX);
			Serial.print(" ");
		}
	}
	else
	{
		for (int i = 0; i < 56; ++i)
		{
			if (i % 8 == 0) {
				Serial.print("|");
			}
			Serial.print(rawData[i] ? "1" : "0");
		}
	}
	
	Serial.print("\n");
}
#else
void Saturn3DSpy::setup() {}

void Saturn3DSpy::loop() {}

void Saturn3DSpy::writeSerial() {}

void Saturn3DSpy::debugSerial() {}

void Saturn3DSpy::updateState() {}

#endif

const char* Saturn3DSpy::startupMsg()
{
	return "Saturn 3D";
}