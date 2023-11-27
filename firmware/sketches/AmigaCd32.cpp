//
// AmigaCd32.cpp
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

#include "AmigaCd32.h"

#if defined(__arm__) && defined(CORE_TEENSY) && defined(ARDUINO_TEENSY35) || (defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO))

#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
#define READ_PINS gpio_get_all()
#define READB_PINS (gpio_get_all() >> 8)
#elif defined(__arm__) && defined(CORE_TEENSY) && defined(ARDUINO_TEENSY35)
#define READ_PINS GPIOD_PDIR
#define READB_PINS GPIOB_PDIR
#endif

void AmigaCd32Spy::setup() {
#if defined(ARDUINO_TEENSY35)
	// GPIOD_PDIR & 0xFF;
	pinMode(2, INPUT_PULLUP);
	pinMode(14, INPUT_PULLUP);
	pinMode(7, INPUT_PULLUP);
	pinMode(8, INPUT_PULLUP);
	pinMode(6, INPUT_PULLUP);
	pinMode(20, INPUT);
	pinMode(21, INPUT_PULLUP);
	pinMode(5, INPUT);

	// GPIOB_PDIR & 0xF;
	pinMode(16, INPUT_PULLUP);
	pinMode(17, INPUT_PULLUP);
	pinMode(19, INPUT_PULLUP);
	pinMode(18, INPUT_PULLUP);
#else
	pinMode(0, INPUT_PULLUP);
	pinMode(1, INPUT_PULLUP);
	pinMode(2, INPUT_PULLUP);
	pinMode(3, INPUT_PULLUP);
	pinMode(4, INPUT_PULLUP);
	pinMode(5, INPUT);
	pinMode(6, INPUT_PULLUP);
	pinMode(7, INPUT);

	// GPIOB_PDIR & 0xF;
	pinMode(8, INPUT_PULLUP);
	pinMode(9, INPUT_PULLUP);
	pinMode(10, INPUT_PULLUP);
	pinMode(11, INPUT_PULLUP);
#endif
}

void AmigaCd32Spy::loop1() 
{
	while (sendRequest)
	{
	}
	noInterrupts();
	updateState();
	sendRequest = true;
	interrupts();
}

void AmigaCd32Spy::loop() 
{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	loop1();
#endif

	if (sendRequest)
	{
		memcpy(sendData, rawData, 9);
		sendRequest = false;
#ifdef DEBUG
		debugSerial();
#else
		writeSerial();
#endif
		T_DELAY(5);
	}
}

void AmigaCd32Spy::writeSerial() {
	for (unsigned char i = 0; i < 9; i++) {
		Serial.write((sendData[i] & 0b11111101));
	}
	Serial.write(SPLIT);
}

void AmigaCd32Spy::debugSerial() {
	for (unsigned char i = 1; i < 8; i++)
	{
		Serial.print((sendData[i] & 0b10000000) == 0 ? 0 : 1);
	}
	Serial.print((sendData[8] & 0b00000001) == 0 ? 0 : 1);
	Serial.print((sendData[0] & 0b00000100) == 0 ? 0 : 1);
	Serial.print((sendData[0] & 0b00001000) == 0 ? 0 : 1);
	Serial.print((sendData[0] & 0b00010000) == 0 ? 0 : 1);
	Serial.println();
}

void AmigaCd32Spy::updateState() {

	//WAIT_FALLING_EDGE(CD32_LATCH)
	while (!PIN_READ(CD32_LATCH)) ;
	do 
	{ 
		rawData[1] = (READ_PINS & 0xFF); 
	} while ((rawData[1] & (1 << CD32_LATCH)) != 0);

	for (int i = 2; i < 8; ++i)
	{
		//WAIT_FALLING_EDGE_CD32(CD32_CLOCK)
		while (!PIN_READ(CD32_CLOCK));
		do 
		{ 
			rawData[i] = (READ_PINS & 0xFF); 
		} while ((rawData[i] & (1 << CD32_CLOCK)) != 0);
	}

	rawData[0] = (byte)(READ_PINS & 0xFF);
	rawData[8] = (byte)(READB_PINS & 0xFF);
}
#else
void AmigaCd32Spy::setup() {
}

void AmigaCd32Spy::loop() {
}

void AmigaCd32Spy::loop1() {
}

void AmigaCd32Spy::writeSerial() {
}

void AmigaCd32Spy::debugSerial() {
}

void AmigaCd32Spy::updateState() {
}
#endif

const char* AmigaCd32Spy::startupMsg()
{
	return "Amiga CD32";
}