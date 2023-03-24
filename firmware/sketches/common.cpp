//
// common.cpp
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

#include "common.h"

void common_pin_setup()
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
#elif defined(__arm__) && defined(CORE_TEENSY)
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
#elif !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	PORTD = 0x00;
	PORTB = 0x00;
	DDRD = 0x00;

	for (int i = 2; i <= 6; ++i)
		pinMode(i, INPUT_PULLUP);
#endif
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Performs a read cycle from a shift register based controller (SNES + NES) using only the data and latch
// wires, and waiting a fixed time between reads.  This read method is deprecated due to being finicky,
// but still exists here to support older builds.
//     latch = Pin index on Port D where the latch wire is attached.
//     data  = Pin index on Port D where the output data wire is attached.
//     bits  = Number of bits to read from the controller.
//  longWait = The NES takes a bit longer between reads to get valid results back.
void read_shiftRegister_2wire(unsigned char rawData[], unsigned char latch, unsigned char data, unsigned char longWait, unsigned char bits)
{
	unsigned char *rawDataPtr = rawData;

	WAIT_FALLING_EDGE(latch);

read_loop:

	// Read the data from the line and store in "rawData"
	*rawDataPtr = !PIN_READ(data);
	++rawDataPtr;
	if (--bits == 0) return;

	// Wait until the next button value is on the data line. ~12us between each.
	asm volatile(
		MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
		MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
		MICROSECOND_NOPS MICROSECOND_NOPS
		);
	if (longWait) {
		asm volatile(
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS
			);
	}

	goto read_loop;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Sends a packet of controller data over the Arduino serial interface.
#pragma GCC optimize ("-O2")
#pragma GCC push_options
void sendRawData(unsigned char rawControllerData[], unsigned char first, unsigned char count)
{
	for (unsigned char i = first; i < first + count; i++) {
		Serial.write(rawControllerData[i] ? ONE : ZERO);
	}
	Serial.write(SPLIT);
}
#pragma GCC pop_options

void sendRawDataDebug(unsigned char rawControllerData[], unsigned char first, unsigned char count)
{
	for (unsigned char i = 0; i < first; i++) {
		Serial.print(rawControllerData[i] ? "1" : "0");
	}
	Serial.print("|");
	int j = 0;
	for (unsigned char i = first; i < first + count; i++) {
		if (j % 8 == 0 && j != 0)
			Serial.print("|");
		Serial.print(rawControllerData[i] ? "1" : "0");
		++j;
	}
	Serial.print("\n");
}

int ScaleInteger(float oldValue, float oldMin, float oldMax, float newMin, float newMax)
{
	float newValue = ((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin) + newMin;
	if (newValue > newMax)
		return newMax;
	if (newValue < newMin)
		return newMin;

	return newValue;
}

int middleOfThree(int a, int b, int c)
{
	// Compare each three number to find middle  
	// number. Enter only if a > b 
	if (a > b)
	{
		if (b > c)
			return b;
		else if (a > c)
			return c;
		else
			return a;
	}
	else
	{
		// Decided a is not greater than b. 
		if (a > c)
			return a;
		else if (b > c)
			return c;
		else
			return b;
	}
}
