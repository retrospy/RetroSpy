//
// DrivingController.cpp
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

#include "DrivingController.h"

#if (defined(TP_PINCHANGEINTERRUPT) && !(defined(__arm__) && defined(CORE_TEENSY))) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
#include <PinChangeInterrupt.h>
#include <PinChangeInterruptBoards.h>
#include <PinChangeInterruptPins.h>
#include <PinChangeInterruptSettings.h>
#endif

// Turn this one for some pretty output.  Do not use with DEBUG!
//#define PRETTY_PRINT

static volatile byte currentState[2];
static volatile byte currentEncoderValue;

static byte lastRawData;

static void bitchange_isr()
{
	byte encoderValue = 0;
	encoderValue |= (READ_PORTD(0b00001100) >> 2);

	if ((currentEncoderValue == 0x3 && encoderValue == 0x2)
		|| currentEncoderValue == 0x2 && encoderValue == 0x0
		|| currentEncoderValue == 0x0 && encoderValue == 0x1
		|| currentEncoderValue == 0x1 && encoderValue == 0x3)
	{
		if (currentState[1] == 0)
			currentState[1] = 15;
		else
			currentState[1] = (currentState[1] - 1) % 16;
	}
	else if (currentEncoderValue == 0x2 && encoderValue == 0x3
		|| currentEncoderValue == 0x3 && encoderValue == 0x1
		|| currentEncoderValue == 0x1 && encoderValue == 0x0
		|| currentEncoderValue == 0x0 && encoderValue == 0x2)
	{
		if (currentState[1] == 15)
			currentState[1] = 0;
		else
			currentState[1] = (currentState[1] + 1) % 16;
	}
	currentEncoderValue = encoderValue;

}

void DrivingControllerSpy::setup(uint8_t cableType)
{
	this->cableType = cableType;
	setup();
}

void DrivingControllerSpy::setup() {

	for (int i = 2; i <= 8; ++i)
		pinMode(i, INPUT_PULLUP);

	currentEncoderValue = 0;
	currentEncoderValue |= READ_PORTD(0b01001100) >> 2;

	currentState[0] = 0;
	currentState[1] = 0;

#ifndef DEBUG
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(2), bitchange_isr, CHANGE);
	attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(3), bitchange_isr, CHANGE);
#else
	attachInterrupt(digitalPinToInterrupt(0), bitchange_isr, CHANGE);
	attachInterrupt(digitalPinToInterrupt(1), bitchange_isr, CHANGE);
#endif
#endif
}

void DrivingControllerSpy::loop() {

	noInterrupts();
	byte rawData = 0;
	rawData |= (READ_PORTD(0xFF) >> 2);
	interrupts();

	byte bitmask = cableType == CABLE_SMS ?  0b00100000 : 0b00010000;
	
#ifndef DEBUG
	currentState[0] = (byte)((rawData & bitmask) == 0);
#endif

#ifdef DEBUG
	if (rawData != lastRawData)
	{
		Serial.print((rawData & 0b00000001) != 0 ? "-" : "1");
		Serial.print((rawData & 0b00000010) != 0 ? "-" : "1");
		Serial.println((rawData & bitmask) != 0 ? "-" : "6");
		lastRawData = rawData;
	}
#else
#ifdef PRETTY_PRINT
	Serial.print(currentState[0]);
	Serial.print("|");
	Serial.print(currentState[1]);
	Serial.print("|");
	Serial.print((rawData & 0b00000001) != 0 ? "-" : "1");
	Serial.print((rawData & 0b00000010) != 0 ? "-" : "1");
	Serial.print("|");
	Serial.println(currentEncoderValue);
#else
	Serial.write(currentState[0]);
	Serial.write(currentState[1] + (byte)65);
	Serial.write('\n');
#endif
#endif
}

void DrivingControllerSpy::writeSerial() {}
void DrivingControllerSpy::debugSerial() {}
void DrivingControllerSpy::updateState() {}

#else
void DrivingControllerSpy::loop() {}
void DrivingControllerSpy::setup() {}
void DrivingControllerSpy::writeSerial() {}
void DrivingControllerSpy::debugSerial() {}
void DrivingControllerSpy::updateState() {}
#endif

const char* DrivingControllerSpy::startupMsg()
{
	return "Atari Driving Controller";
}