//
// N64.cpp
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

#include "N64.h"

#if defined(__arm__) && defined(CORE_TEENSY) && (defined(ARDUINO_TEENSY35) || defined(ARDUINO_TEENSY40) || defined(ARDUINO_TEENSY41))

void N64Spy::loop() 
{
	unsigned char *rawDataPtr = rawData;
	elapsedMicros betweenLowSignal = 0;
	short headerVal = 0;
	int headerBits = 8;
	
findcmdinit:
	interrupts();

	rawDataPtr = rawData;
	
	// Wait for the line to go high then low.
	WAIT_FALLING_EDGE(N64_PIN);
	if (betweenLowSignal < 25)
	{
		betweenLowSignal = 0;
		goto findcmdinit;
	}
	else
	{
		headerBits = 7;
		betweenLowSignal = 0;
		
		noInterrupts();
		// Wait ~2us between line reads
		asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);

		// Read a bit from the line and store as a byte in "rawData"
		*rawDataPtr = PIN_READ(N64_PIN);
		headerVal = (*rawDataPtr != 0 ? 0x80 : 0x00);
		++rawDataPtr;

		goto readCmd;
	}
	
	goto findcmdinit;
	
readCmd:
	
	// Wait for the line to go high then low.
	WAIT_FALLING_EDGE(N64_PIN);

	// Wait ~2us between line reads
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);

	// Read a bit from the line and store as a byte in "rawData"
	*rawDataPtr = PIN_READ(N64_PIN);
	
	headerVal |= *rawDataPtr != 0 ? (1 << (headerBits - 1)) : 0;

	++rawDataPtr;
	if (--headerBits == 0)
	{
		if (headerVal == 0x00)
		{
			readBits = 26;
			goto readData;
		}
		if (headerVal == 0x01)
		{
			readBits = 34;
			goto readData;
		}
		if (headerVal == 0x03)
		{
			readBits = 266;
			goto readData;
		}
		if (headerVal == 0x04)
		{
			readBits = 266;
			goto readData;
		}
		if (headerVal == 0xff)
		{
			readBits = 26;
			goto readData;
		}
		else
		{
#if defined(DEBUG)
			//Serial.println(headerVal);
#endif
			interrupts();
			betweenLowSignal = 0;
			goto findcmdinit;
		}
	}
	goto readCmd;
	
readData:
	
	// Wait for the line to go high then low.
	WAIT_FALLING_EDGE(N64_PIN);
	
	// Wait ~2us between line reads
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);

	// Read a bit from the line and store as a byte in "rawData"
	*rawDataPtr = PIN_READ(N64_PIN);
	
	rawDataPtr++;
	
	if (--readBits == 0)
	{
		goto printData;
	}
	
	goto readData;
	
printData:
	interrupts();
	if (headerVal == 0x01)
	{
		
	
#if !defined(DEBUG)
	
		writeSerial();
#else
		debugSerial();
#endif
	}
	betweenLowSignal = 0;
	goto findcmdinit;
}

void N64Spy::updateState() {

}

void N64Spy::writeSerial() {
	const unsigned char first = 9;

	for (unsigned char i = first; i < first + N64_BITCOUNT; i++) {
		Serial.write(rawData[i] ? ONE : ZERO);
	}
	Serial.write(SPLIT);
}

void N64Spy::debugSerial() {
	
	int j = 0;
	const unsigned char first = 9;
	for (unsigned char i = first; i < first + N64_BITCOUNT; i++) {
		if (j % 8 == 0 && j != 0)
			Serial.print("|");
		Serial.print(rawData[i] ? "1" : "0");
		j++;
	}
	Serial.print("\n");
}

#elif defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E)

static bool getControllerInfo = false;

void N64Spy::loop() {
	noInterrupts();
	updateState();
	interrupts();
	if (checkPrefixN64()) {
#if !defined(DEBUG)
	writeSerial();
#else
		debugSerial();
#endif
	}
	else {
		// This makes no sense, but its needed after command 0x0 or else you get garbage on the line
		A_DELAY(2);
	}
	T_DELAY(5);
}

// Verifies that the 9 bits prefixing N64 controller data in 'rawData'
// are actually indicative of a controller state signal.
inline bool N64Spy::checkPrefixN64() {
	return rawData[0] == 0x01;
}

void N64Spy::updateState() {
	unsigned short bits;
	bool shortcutToControllerPoll = false;
	getControllerInfo = false;
	
	unsigned char *rawDataPtr = &rawData[1];
	byte /*bit7, bit6, bit5, bit4, bit3, */bit2, bit1, bit0;
	WAIT_FALLING_EDGE(N64_PIN);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
	// bit7 = PIND & 0b00000100;
	WAIT_FALLING_EDGE(N64_PIN);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
	// bit6 = PIND & 0b00000100;
	WAIT_FALLING_EDGE(N64_PIN);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
	// bit5 = PIND & 0b00000100;
	WAIT_FALLING_EDGE(N64_PIN);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
	// bit4 = PIND & 0b00000100;
	WAIT_FALLING_EDGE(N64_PIN);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
	// bit3 = PIND & 0b00000100;
	WAIT_FALLING_EDGE(N64_PIN);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
	bit2 = READ_PORTD(0b00000100);
	if (bit2 != 0)  // Controller Reset
	{
		WAIT_FALLING_EDGE(N64_PIN);
		asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
		// bit1 = PIND & 0b00000100;
		WAIT_FALLING_EDGE(N64_PIN);
		asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
		// bit0 = PIND & 0b00000100;
		bits = 25;
		rawData[0] = 0xFF;
		goto read_loop;
	}
	WAIT_FALLING_EDGE(N64_PIN);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
	bit1 = READ_PORTD(0b00000100);
	if (bit1 != 0) // read or write to memory pack (this doesn't work correctly)
	{
		WAIT_FALLING_EDGE(N64_PIN);
		asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
		// bit0 = PIND & 0b00000100;
		bits = 281;
		rawData[0] = 0x02;
		goto read_loop;
	}
checkControllerPoll:
	WAIT_FALLING_EDGE(N64_PIN);
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);
	bit0 = READ_PORTD(0b00000100);
	if (bit0 != 0) // controller poll
		{
			bits = 33;
			rawData[0] = 0x01;
			goto read_loop;
		}
	bits = 25;     // Get controller info
	rawData[0] = 0x00;
	getControllerInfo = true;

read_loop:
	
	// Wait for the line to go high then low.
	WAIT_FALLING_EDGE(N64_PIN);

	// Wait ~2us between line reads

	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);

	// Read a bit from the line and store as a byte in "rawData"
	*rawDataPtr = READ_PORTD(0b00000100);
	++rawDataPtr;
	--bits;
	if (bits == 0)
	{
		if (shortcutToControllerPoll) 
		{   
			rawDataPtr = &rawData[1];
			shortcutToControllerPoll = false;
			goto checkControllerPoll;
		}
		else if (rawData[0] == 0x00 && rawData[2] == 0 && rawData[3] == 0 && rawData[4] == 0 && rawData[5] == 0
			&& rawData[6] == 0 && rawData[7] != 0 && rawData[8] == 0 && rawData[9] != 0 && rawData[25] != 0) 
		{
			shortcutToControllerPoll = true;
			bits = 8;
		}
		else if (rawData[0] == 0x00 && rawData[2] == 0 && rawData[3] == 0 && rawData[4] == 0 && rawData[5] == 0
			&& rawData[6] == 0 && rawData[7] != 0 && rawData[8] == 0 && rawData[9] != 0 && rawData [24] == 0 && rawData[25] == 0) 
		{
			shortcutToControllerPoll = true;
			bits = 8;
		}
		else 
			return;
	}	

	goto read_loop;
}
void N64Spy::writeSerial() {
	const unsigned char first = getControllerInfo ? 1 : 2;

	for (unsigned char i = first; i < first + N64_BITCOUNT; i++) {
		Serial.write(rawData[i] ? ONE : ZERO);
	}
	Serial.write(SPLIT);
}

void N64Spy::debugSerial() {
	Serial.print(rawData[0]);
	Serial.print("|");
	int j = 0;
	const unsigned char first = getControllerInfo ? 1 : 2;
	for (unsigned char i = 0; i < 32; i++) {
		if (j % 8 == 0 && j != 0)
			Serial.print("|");
		Serial.print(rawData[i + first] ? "1" : "0");
		j++;
	}
	Serial.print("\n");
}

#else

void N64Spy::loop() {}
void N64Spy::writeSerial() {}
void N64Spy::debugSerial() {}
void N64Spy::updateState() {}

#endif

const char* N64Spy::startupMsg()
{
	return "N64";
}