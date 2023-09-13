//
// FMTownsKeyboardAndMouse.cpp
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

// ---------- Uncomment this for a mostly complete general purpose 9200 baud 8E1 serial sniffer  --------------
//#define SNIFFER

#include "FMTownsKeyboardAndMouse.h"

#if (defined(__arm__) && defined(CORE_TEENSY) && defined(ARDUINO_TEENSY35)) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

static byte rawData[32];
static byte mouseData[4];

static unsigned long lastStrobe = 0;
static unsigned int strobeCount = 0;

static byte controlModeOffset = 0;
static byte seenControlModeOff = false;

void FMTownsKeyboardAndMouseSpy::loop()
{
	byte incomingByte;

#ifdef SNIFFER
	if (Serial1.available() > 0)
	{
		incomingByte = Serial1.read();
		Serial.print("UART received: ");
		Serial.println(incomingByte, HEX);
	}
	Serial.printf(mouseData[2] == 0 ? "0" : "1");
	Serial.printf(mouseData[3] == 0 ? "0" : "1");
	Serial.print(mouseData[0]);
	Serial.print("|");
	Serial.println(mouseData[1]);
	delay(5);
#else
	if (Serial1.available() > 1)
	{
		incomingByte = Serial1.read();
		if (incomingByte == 0xA0)
		{
			incomingByte = Serial1.read();
			if (incomingByte < 0x7E)
			{
				incomingByte += controlModeOffset;
				rawData[incomingByte / 8] |= (1 << (incomingByte % 8));
			}
			else if (incomingByte == 0x7E)
				controlModeOffset = 0x80;
		}
		else if (incomingByte == 0xB0)
		{
			incomingByte = Serial1.read();
			if (incomingByte < 0x7E)
			{
				incomingByte += controlModeOffset;
				rawData[incomingByte / 8] &= ~(1 << (incomingByte % 8));
				if (seenControlModeOff)
				{
					seenControlModeOff = false;
					controlModeOffset = 0;
				}
			}
			else if (incomingByte == 0x7E)
				seenControlModeOff = true;
			else if (incomingByte == 0x7F)
			{
				for (int i = 0; i < 32; ++i)
					rawData[i] = 0;
			}
		}
		else if (incomingByte == 0xF2)
		{
			incomingByte = Serial1.read();
			incomingByte += 0x2d;
			rawData[incomingByte / 8] |= (1 << (incomingByte % 8));
		}
		else if (incomingByte == 0xFA)
		{
			incomingByte = Serial1.read();
		}
		else if (incomingByte == 0xF0)
		{
			incomingByte = Serial1.read();
		}
		else if (incomingByte == 0xF1)
		{
			incomingByte = Serial1.read();
			incomingByte += 0x2d;
			rawData[incomingByte / 8] &= ~(1 << (incomingByte % 8));
		}
  }

#ifndef DEBUG
	byte expandedRawData[71];
	for (int i = 0; i < 32; ++i)
	{
		expandedRawData[i * 2] = ((rawData[i] & 0x0F) << 4);
		expandedRawData[i * 2 + 1] = (rawData[i] & 0xF0);
	}
	expandedRawData[64] = ((mouseData[0] & 0x0F) << 4);
	expandedRawData[65] = (mouseData[0] & 0xF0);
	expandedRawData[66] = ((mouseData[1] & 0x0F) << 4);
	expandedRawData[67] = (mouseData[1] & 0xF0);
	expandedRawData[68] = mouseData[2];
	expandedRawData[69] = mouseData[3];
	expandedRawData[70] = '\n';
	Serial.write(expandedRawData, 71);
#else
	for (int i = 0; i < 32; ++i)
	{
		Serial.print(rawData[i], HEX);
		Serial.print(" ");
	}
	Serial.print("|");
	Serial.print(mouseData[2] == 0 ? "0" : "1");
	Serial.print(mouseData[3] == 0 ? "0" : "1");
	Serial.print("|");
	Serial.print(mouseData[0]);
	Serial.print("|");
	Serial.print(mouseData[1]);
	Serial.println();
#endif

	delay(5);
#endif
}

void FMTownsKeyboardAndMouseSpy::writeSerial() {}
void FMTownsKeyboardAndMouseSpy::debugSerial() {}
void FMTownsKeyboardAndMouseSpy::updateState() {}

#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
#define READ_PORTD_NO_SHIFT(mask) (gpio_get_all() & mask)
#else
#define READ_PORTD_NO_SHIFT(mask) (GPIOD_PDIR & mask)
#endif

void strobe() {
	unsigned long currentStrobe = micros();
	if (currentStrobe - lastStrobe > 200)
	{
		strobeCount = 0;
		mouseData[0] = 0;
		mouseData[1] = 0;
	}
	else
		++strobeCount;

	if (strobeCount == 0)
		mouseData[0] |= ((READ_PORTD_NO_SHIFT(0b00111100)) >> 2);
	else if (strobeCount == 1)
		mouseData[1] |= ((READ_PORTD_NO_SHIFT(0b00111100)) << 2);
	else if (strobeCount == 2)
		mouseData[1] |= ((READ_PORTD_NO_SHIFT(0b00111100)) >> 2);
	else if (strobeCount == 3)
	{
		mouseData[2] = digitalRead(FMTOWNS_MOUSE_BUTTON_1) == HIGH ? 0 : 1;
		mouseData[3] = digitalRead(FMTOWNS_MOUSE_BUTTON_2) == HIGH ? 0 : 1;
		mouseData[0] |= ((READ_PORTD_NO_SHIFT(0b00111100)) << 2);
	}

	lastStrobe = currentStrobe;
}

void FMTownsKeyboardAndMouseSpy::setup()
{
	for (int i = 0; i < 32; ++i)
		rawData[i] = 0;

#if (defined(__arm__) && defined(CORE_TEENSY) && defined(ARDUINO_TEENSY35))	
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
#else
	for (int i = 0; i < 12; ++i)
		pinMode(i, INPUT_PULLUP);
#endif
	
	attachInterrupt(digitalPinToInterrupt(FMTOWNS_MOUSE_STROBE), strobe, CHANGE);
	Serial1.begin(9600, SERIAL_8E1);

}

#else
void FMTownsKeyboardAndMouseSpy::loop(){}
void FMTownsKeyboardAndMouseSpy::writeSerial() {}
void FMTownsKeyboardAndMouseSpy::debugSerial() {}
void FMTownsKeyboardAndMouseSpy::updateState() {}
void FMTownsKeyboardAndMouseSpy::setup(){}
#endif

const char* FMTownsKeyboardAndMouseSpy::startupMsg()
{
	return "FM Towns Keyboard & Mouse";
}