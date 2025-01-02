//
// Duo.cpp
//
// Author:
//       Christopher "Zoggins" Mallery <zoggins@retro-spy.com>
//
// Copyright (c) 2025 RetroSpy Technologies
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

#include "Duo.h"

#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

void DuoSpy::loop() 
{
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

void DuoSpy::updateState() {
	word temp = 0;
	currentState = 0;
try_again: 

	// Get U, D, L & R
	WAIT_LEADING_EDGE(4);
	delayMicroseconds(1);
	temp = READ_PORTD(0b01111100);
	if ((temp & 0b00111100) == 0 || (temp & 0b01000000) == 0)
		goto try_again;
	currentState |= (temp & 0b00111100)  >> 2;
	
	// Get Run, Select, I and II
	while (READ_PORTD(0b01000000) != 0) ;
	delayMicroseconds(1);
	temp = READ_PORTD(0b01111100);
	if ((temp & 0b01000000) != 0)
		goto try_again;
	currentState |= (temp & 0b00111100) << 2;
	
	// Get III, IV, V & VI 
	// Or it will grab Run, Select, I and II again for 2 button controllers
	while (READ_PORTD(0b01111100) != 0b01000000) ;
	while (READ_PORTD(0b01000000) != 0) ;
	delayMicroseconds(1);
	temp = READ_PORTD(0b01111100);
	if ((temp & 0b01000000) != 0)
		goto try_again;
	currentState |= (temp & 0b00111100)  << 6;
	
	currentState = ~currentState;
}

void DuoSpy::writeSerial() {
	for (unsigned char i = 0; i < 12; ++i) {
		Serial.write(currentState & (1 << i) ? ONE : ZERO);
	}
	Serial.write(SPLIT);
}

void DuoSpy::debugSerial() {
	Serial.print((currentState & 0b0000000000000001) ? "U" : "0");
	Serial.print((currentState & 0b0000000000000010) ? "R" : "0");
	Serial.print((currentState & 0b0000000000000100) ? "D" : "0");
	Serial.print((currentState & 0b0000000000001000) ? "L" : "0");
	Serial.print((currentState & 0b0000000000010000) ? "A" : "0");
	Serial.print((currentState & 0b0000000000100000) ? "B" : "0");
	Serial.print((currentState & 0b0000000001000000) ? "S" : "0");
	Serial.print((currentState & 0b0000000010000000) ? "R" : "0");
	Serial.print((currentState & 0b0000000100000000) ? "3" : "0");
	Serial.print((currentState & 0b0000001000000000) ? "4" : "0");
	Serial.print((currentState & 0b0000010000000000) ? "5" : "0");
	Serial.print((currentState & 0b0000100000000000) ? "6" : "0");
	Serial.print("\n");
}

#else
void DuoSpy::loop() {}

void DuoSpy::writeSerial() {}

void DuoSpy::debugSerial() {}

void DuoSpy::updateState() {}

#endif

const char* DuoSpy::startupMsg()
{
	return "Analogue Duo";
}