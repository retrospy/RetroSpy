//
// VFlash.cpp
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

#include "VFlash.h"

#if (!defined(TP_PINCHANGEINTERRUPT) && !defined(ESP_PLATFORM))

#if !defined(ARDUINO_TEENSY40) && !defined(ARDUINO_TEENSY41) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO) 
#include <SoftwareSerial.h>
static SoftwareSerial mySerial(2, 3); // RX,TX 
#endif

static byte rawData[120];
static byte x, y;
static byte buttons[15];

void VFlashSpy::setup() {

#if !defined(ARDUINO_TEENSY40) && !defined(ARDUINO_TEENSY41) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	mySerial.begin(9600);
#else
	Serial1.begin(9600);
#endif
}

void VFlashSpy::loop() {
	updateState();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
}

void VFlashSpy::writeSerial() {

	for (int i = 0; i < 15; ++i)
		Serial.write(buttons[i] != 0 ? ONE : ZERO);

	Serial.write(x == 10 ? 11 : x);
	Serial.write(y == 10 ? 11 : y);
	Serial.write(SPLIT);
}

void VFlashSpy::debugSerial() {

	for (int i = 0; i < 15; ++i)
		Serial.print(buttons[i] != 0 ? "1" : "0");

	Serial.print("|");
	Serial.print(x);
	Serial.print("|");
	Serial.println(y);
}

#if !defined(ARDUINO_TEENSY40) && !defined(ARDUINO_TEENSY41) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
int VFlashSpy::available()
{
	return mySerial.available();
}

char VFlashSpy::read()
{
	return mySerial.read();
}
#else
int VFlashSpy::available()
{
	return Serial1.available();
}

char VFlashSpy::read()
{
	return Serial1.read();
}
#endif

void VFlashSpy::updateState() {
	int count = 0;
	byte c = 0;
	do {

		if (available())
		{
			c = read();
		}
	} while (c != 255);

	for (int i = 0; i < 8; ++i)
		rawData[count++] = (c & (1 << i)) == 0 ? 0 : 1;
      
	while (available() < 14) {};
	
	for (int i = 0; i < 14; ++i)
	{
		c = read();      
		for (int j = 0; j < 8; ++j)
		{
			rawData[count++] = (c & (1 << j)) == 0 ? ZERO : ONE;
		}

	}
	
	buttons[0] = rawData[58];
	buttons[1] = rawData[59];
	buttons[2] = rawData[60];
	buttons[3] = rawData[61];
	buttons[4] = rawData[64];
	buttons[5] = rawData[65];
	buttons[6] = rawData[66];
	buttons[7] = rawData[67];
	buttons[8] = rawData[68];
	buttons[9] = rawData[69];
	buttons[10] = rawData[72];
	buttons[11] = rawData[73];
	buttons[12] = rawData[74];
	buttons[13] = rawData[75];
	buttons[14] = rawData[80];
    
	x = 0;
	x |= rawData[43]		!= 0 ? 1 : 0;
	x |= rawData[43 + 1]	!= 0 ? 2 : 0;
	x |= rawData[43 + 2]	!= 0 ? 4 : 0;
	x |= rawData[48]		!= 0 ? 8 : 0;
	x |= rawData[48 + 1]	!= 0 ? 16 : 0;
	x |= rawData[48 + 2]	!= 0 ? 32 : 0;
	x |= rawData[48 + 3]	!= 0 ? 64 : 0;
	x |= rawData[56]		!= 0 ? 128 : 0;      

	y = 0;
	y |= rawData[32]		!= 0 ? 1 : 0; 
	y |= rawData[32 + 1]	!= 0 ? 2 : 0;
	y |= rawData[32 + 2]	!= 0 ? 4 : 0;
	y |= rawData[32 + 3]	!= 0 ? 8 : 0;
	y |= rawData[32 + 4]	!= 0 ? 16 : 0;
	y |= rawData[32 + 5]	!= 0 ? 32 : 0;
	y |= rawData[40]		!= 0 ? 64 : 0;
	y |= rawData[41]		!= 0 ? 128 : 0;
}

#else

void VFlashSpy::setup() {
}

void VFlashSpy::writeSerial() {
}

void VFlashSpy::debugSerial() {
}

void VFlashSpy::loop() {
}

void VFlashSpy::updateState() {
}

#endif

const char* VFlashSpy::startupMsg()
{
	return "V.Flash";
}