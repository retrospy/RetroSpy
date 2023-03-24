//
// Jaguar.cpp
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

#include "Jaguar.h"

#if !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)

void JaguarSpy::loop() {
	noInterrupts();
	updateState();
	interrupts();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
	delay(1);
}

void JaguarSpy::updateState() {
	WAIT_FALLING_EDGEB(AJ_COLUMN1);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS);
	rawData[3] = (READ_PORTD(0b11111000));

	WAIT_FALLING_EDGEB(AJ_COLUMN2);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS);
	rawData[2] = (READ_PORTD(0b11111000));

	WAIT_FALLING_EDGEB(AJ_COLUMN3);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS);
	rawData[1] = (READ_PORTD(0b11111000));

	WAIT_FALLING_EDGEB(AJ_COLUMN4);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS);
	rawData[0] = (READ_PORTD(0b11111100));
}

void JaguarSpy::writeSerial() {
	Serial.write(rawData[0]);
	Serial.write(rawData[1]);
	Serial.write(rawData[2]);
	Serial.write(rawData[3]);
	Serial.write(SPLIT);
}

void JaguarSpy::debugSerial() {
	Serial.print((rawData[0] & 0b00000100) == 0 ? "P" : "0");
	Serial.print((rawData[0] & 0b00001000) == 0 ? "A" : "0");
	Serial.print((rawData[0] & 0b00010000) == 0 ? "R" : "0");
	Serial.print((rawData[0] & 0b00100000) == 0 ? "L" : "0");
	Serial.print((rawData[0] & 0b01000000) == 0 ? "D" : "0");
	Serial.print((rawData[0] & 0b10000000) == 0 ? "U" : "0");

	//Serial.print((rawData[1] & 0b00000100) == 0 ? "P" : "0");
	Serial.print((rawData[1] & 0b00001000) == 0 ? "B" : "0");
	Serial.print((rawData[1] & 0b00010000) == 0 ? "1" : "0");
	Serial.print((rawData[1] & 0b00100000) == 0 ? "4" : "0");
	Serial.print((rawData[1] & 0b01000000) == 0 ? "7" : "0");
	Serial.print((rawData[1] & 0b10000000) == 0 ? "*" : "0");

	//Serial.print((rawData[2] & 0b00000100) == 0 ? "P" : "0");
	Serial.print((rawData[2] & 0b00001000) == 0 ? "C" : "0");
	Serial.print((rawData[2] & 0b00010000) == 0 ? "2" : "0");
	Serial.print((rawData[2] & 0b00100000) == 0 ? "5" : "0");
	Serial.print((rawData[2] & 0b01000000) == 0 ? "8" : "0");
	Serial.print((rawData[2] & 0b10000000) == 0 ? "Z" : "0");

	//Serial.print((rawData[3] & 0b00000100) == 0 ? "P" : "0");
	Serial.print((rawData[3] & 0b00001000) == 0 ? "O" : "0");
	Serial.print((rawData[3] & 0b00010000) == 0 ? "3" : "0");
	Serial.print((rawData[3] & 0b00100000) == 0 ? "6" : "0");
	Serial.print((rawData[3] & 0b01000000) == 0 ? "9" : "0");
	Serial.print((rawData[3] & 0b10000000) == 0 ? "#" : "0");

	Serial.print("\n");
}
#else

void JaguarSpy::loop() {}

void JaguarSpy::writeSerial() {}

void JaguarSpy::debugSerial() {}

void JaguarSpy::updateState() {}

#endif

const char* JaguarSpy::startupMsg()
{
	return "Jaguar";
}