//
// AmgiaKeyboard.cpp
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

// ---------- Uncomment these for debugging modes --------------
//#define SNIFFER


#include "AmigaKeyboard.h"

#if !(defined(__arm__) && defined(CORE_TEENSY))

#define BUFFER_SIZE 45
static volatile uint8_t buffer[BUFFER_SIZE];
static volatile uint8_t head, tail;

#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
static int DataPin = 2;
const int IRQpin = 1;
#else
static int DataPin = 4;
const int IRQpin = 3;
#endif

void clockInterrupt(void)
{
	static uint8_t bitcount = 0;
	static uint8_t incoming = 0;
	static uint32_t prev_ms = 0;
	uint32_t now_ms;
	uint8_t val;

	val = digitalRead(DataPin);
	now_ms = micros();
	if (now_ms - prev_ms > 100) {
		bitcount = 0;
		incoming = 0;
	}
	prev_ms = now_ms;
	if (bitcount < 7) {
		incoming |= (val << (6 - bitcount));
	}
	else
	{
		incoming |= (val << 7);
	}
	bitcount++;
	if (bitcount == 8) {
		uint8_t i = head + 1;
		if (i >= BUFFER_SIZE) i = 0;
		if (i != tail) {
			buffer[i] = ~incoming;
			head = i;
		}
		bitcount = 0;
		incoming = 0;
	}
}

static inline uint8_t get_scan_code(void)
{
	uint8_t c, i;

	i = tail;
	if (i == head) return 0xFF;
	i++;
	if (i >= BUFFER_SIZE) i = 0;
	c = buffer[i];
	tail = i;
	return c;
}

byte rawData[16];

void AmigaKeyboardSpy::setup()
{

	for (int i = 0; i < 16; ++i)
		rawData[i] = 0;

	// initialize the pins
	pinMode(IRQpin, INPUT_PULLUP);
	pinMode(DataPin, INPUT_PULLUP);

	head = 0;
	tail = 0;
	attachInterrupt(1, clockInterrupt, FALLING);
}

void AmigaKeyboardSpy::loop()
{
	uint8_t s;

	while (1) {
		s = get_scan_code();
		if (s != 0xFF)
		{
#ifdef SNIFFER
			Serial.print((s & 0b01111111), HEX);
			if ((s & 0b10000000) == 0)
				Serial.println(" depressed");
			else
				Serial.println(" pressed");
#endif
			if ((s & 0b10000000) == 0)
			{
				rawData[(s & 0b01111111) / 8] |= (1 << ((s & 0b01111111) % 8));
			}
			else
			{
				rawData[(s & 0b01111111) / 8] &= ~(1 << ((s & 0b01111111) % 8));
			}
		}
#ifndef SNIFFER
#ifndef DEBUG
		int checksum = 0;
		for (int i = 0; i < 16; i++)
		{
			checksum += rawData[i];
			Serial.write(((rawData[i] & 0x0F) << 4));
			Serial.write(rawData[i] & 0xF0);
		}
		Serial.write((checksum & 0x000f) << 4);
		Serial.write(checksum & 0x00f0);
		Serial.write((checksum & 0x0f00) >> 4);
		Serial.write((checksum & 0xf000) >> 8);
		Serial.write('\n');
#else
		for (int i = 0; i < 16; i++)
		{
			Serial.print(rawData[i]);
			Serial.print(" ");
		}
		Serial.print("\n");
#endif
#endif
	}

}

void AmigaKeyboardSpy::writeSerial() {}

void AmigaKeyboardSpy::debugSerial() {}

void AmigaKeyboardSpy::updateState() {}

#else
void AmigaKeyboardSpy::setup() {}

void AmigaKeyboardSpy::loop() {}

void AmigaKeyboardSpy::writeSerial() {}

void AmigaKeyboardSpy::debugSerial() {}

void AmigaKeyboardSpy::updateState() {}

#endif

const char* AmigaKeyboardSpy::startupMsg()
{
	return "Amiga Keyboard";
}