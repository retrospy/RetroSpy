//
// VSmile.cpp
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

#include "VSmile.h"

#if defined(__arm__) && defined(CORE_TEENSY) && (defined(ARDUINO_TEENSY35) || defined(ARDUINO_TEENSY40))

static bool redButton = false;
static bool yellowButton = false;
static bool blueButton = false;
static bool greenButton = false;
static bool enterButton = false;
static bool helpButton = false;
static bool exitButton = false;
static bool learningZoneButton = false;
static byte x = 0;
static byte y = 0;

void VSmileSpy::setup() {
	
	Serial1.begin(4800);
	Serial2.begin(4800);
	
}

void VSmileSpy::loop() {
	updateState();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
	delay(5);
}

void VSmileSpy::writeSerial() {
	Serial.write(redButton ? ONE : ZERO);
	Serial.write(yellowButton ? ONE : ZERO);
	Serial.write(blueButton ? ONE : ZERO);
	Serial.write(greenButton ? ONE : ZERO);
	Serial.write(enterButton ? ONE : ZERO);
	Serial.write(helpButton ? ONE : ZERO);
	Serial.write(exitButton ? ONE : ZERO);
	Serial.write(learningZoneButton ? ONE : ZERO);
	Serial.write(x == 10 ? 11 : x);
	Serial.write(y == 10 ? 11 : y);
	Serial.write(SPLIT);
}

void VSmileSpy::debugSerial() {
	Serial.print(redButton ? "1" : "0");
    Serial.print(yellowButton ? "1" : "0");
    Serial.print(blueButton ? "1" : "0");
    Serial.print(greenButton ? "1" : "0");
    Serial.print(enterButton ? "1" : "0");
    Serial.print(helpButton ? "1" : "0");
    Serial.print(exitButton ? "1" : "0");
    Serial.print(learningZoneButton ? "1" : "0");
    Serial.print("|");
    Serial.print(x);
    Serial.print("|");
    Serial.println(y);
}

void VSmileSpy::updateState() {
	if (Serial1.available())
	{
		char c = Serial1.read();
		if ((c & 0b11110000) == 0b11000000)
		{
			x = (c & 0b00001111);
		}
		if ((c & 0b11110000) == 0b10000000)
		{
			y = (c & 0b00001111);
		}
		if ((c & 0b11110000) == 0b10100000)
		{
			if ((c & 0b00001111) == 0b00000011)
			{
				helpButton = true;
				enterButton = false;
				exitButton = false;
				learningZoneButton = false;
			}
			else if ((c & 0b00001111) == 0b00000001)
			{
				helpButton = false;
				enterButton = true;
				exitButton = false;
				learningZoneButton = false;
			}
			else if ((c & 0b00001111) == 0b00000010)
			{
				helpButton = false;
				enterButton = false;
				exitButton = true;
				learningZoneButton = false;
			}
			else if ((c & 0b00001111) == 0b00000100)
			{
				helpButton = false;
				enterButton = false;
				exitButton = false;
				learningZoneButton = true;
			}
			else 
			{ 
				helpButton = false;
				enterButton = false;
				exitButton = false;
				learningZoneButton = false;                 
			}
		}
	}
	if (Serial2.available())
	{
		char c = Serial2.read();
		if ((c & 0b11110000) == 0b01100000)
		{
			redButton = ((c & 0b00001000) != 0) ? true : false;
			yellowButton = ((c & 0b00000100) != 0) ? true : false;
			blueButton = ((c & 0b00000010) != 0) ? true : false;
			greenButton = ((c & 0b00000001) != 0) ? true : false;
		}
	}
}

#else

void VSmileSpy::setup() {
}

void VSmileSpy::loop() {
}

void VSmileSpy::writeSerial() {
}

void VSmileSpy::debugSerial() {
}

void VSmileSpy::updateState() {
}

#endif

const char* VSmileSpy::startupMsg()
{
	return "V.Smile";
}