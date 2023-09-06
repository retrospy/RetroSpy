//
// GC.h
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

#include "GC.h"

#if (defined(__arm__) && defined(CORE_TEENSY) && (defined(ARDUINO_TEENSY35) || defined(ARDUINO_TEENSY40) || defined(ARDUINO_TEENSY41))) || (defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO))
static int show = 0;

static u_int8_t dummyStickData[] = {
	ONE,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ONE,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ONE,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ONE,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	ZERO,
	SPLIT
};

void GCSpy::loop() 
{
	unsigned char *rawDataPtr = rawData;
	elapsedMicros betweenLowSignal = 0;
	short headerVal = 0;
	int headerBits = 8;
	
findcmdinit:
	interrupts();

	rawDataPtr = rawData;
	
	// Wait for the line to go high then low.
	WAIT_FALLING_EDGE(GC_PIN);
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
		*rawDataPtr = PIN_READ(GC_PIN);
		headerVal = (*rawDataPtr != 0 ? 0x80 : 0x00);
		++rawDataPtr;

		goto readCmd;
	}
	
	goto findcmdinit;
	
readCmd:
	
	// Wait for the line to go high then low.
	WAIT_FALLING_EDGE(GC_PIN);

	// Wait ~2us between line reads
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);

	// Read a bit from the line and store as a byte in "rawData"
	*rawDataPtr = PIN_READ(GC_PIN);
	
	headerVal |= *rawDataPtr != 0 ? (1 << (headerBits - 1)) : 0;

	++rawDataPtr;
	if (--headerBits == 0)
	{
		if (headerVal == 0x40)
		{
			readBits = 82;
			goto readData;
		}
		if (headerVal == 0x54)
		{
			readBits = 82;
			goto readData;
		}
		if (headerVal == 0x14)
		{
			readBits = 25;
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
	WAIT_FALLING_EDGE(GC_PIN);
	
	// Wait ~2us between line reads
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);

	// Read a bit from the line and store as a byte in "rawData"
	*rawDataPtr = PIN_READ(GC_PIN);
	
	rawDataPtr++;
	
	if (--readBits == 0)
	{
		goto printData;
	}
	
	goto readData;
	
printData:
	interrupts();
#if !defined(DEBUG)
	if (headerVal == 0x40)
		sendRawData(rawData, GC_PREFIX, (rawData[14] | rawData[15]) == 0 ? GC_BITCOUNT - 8 : GC_BITCOUNT);
	else if (headerVal == 0x14 && ++show % 2 == 0)  // Gameboy Player polls too damn many times, slows down display.
		writeSerial();  								// This doesn't seem to negatively affect other games.
	else if(headerVal == 0x54)
		writeKeyboard();
#else
	if (headerVal == 0x54)
		debugKeyboard();
	else if (headerVal == 0x14)
		debugSerial();
	else
		sendRawDataDebug(rawData, 0, GC_BITCOUNT + GC_PREFIX);
#endif
	betweenLowSignal = 0;
	goto findcmdinit;
}

void GCSpy::updateState() {

}

void GCSpy::writeSerial() {
	Serial.write(ZERO);
	Serial.write(ZERO);
	Serial.write(ZERO);
	Serial.write(rawData[21] ? ONE : ZERO);
	Serial.write(ZERO);
	Serial.write(ZERO);
	Serial.write(rawData[23] ? ONE : ZERO);
	Serial.write(rawData[24] ? ONE : ZERO);
	Serial.write(ZERO);
	Serial.write(rawData[31] ? ONE : ZERO);
	Serial.write(rawData[32] ? ONE : ZERO);
	Serial.write(rawData[22] ? ONE : ZERO);
	Serial.write(rawData[18] ? ONE : ZERO);
	Serial.write(rawData[17] ? ONE : ZERO);
	Serial.write(rawData[20] ? ONE : ZERO);
	Serial.write(rawData[19] ? ONE : ZERO);
	Serial.write(dummyStickData, 49);
}

void GCSpy::debugSerial() {
	Serial.print("0");
	Serial.print("0");
	Serial.print("0");
	Serial.print(rawData[21] ? "t" : "0");
	Serial.print("0");
	Serial.print("0");
	Serial.print(rawData[23] ? "b" : "0");
	Serial.print(rawData[24] ? "a" : "0");
	Serial.print("0");
	Serial.print(rawData[31] ? "L" : "0");
	Serial.print(rawData[32] ? "R" : "0");
	Serial.print(rawData[22] ? "s" : "0");
	Serial.print(rawData[18] ? "u" : "0");
	Serial.print(rawData[17] ? "d" : "0");
	Serial.print(rawData[20] ? "l" : "0");
	Serial.print(rawData[19] ? "r" : "0");
	Serial.print(128);
	Serial.print(128);
	Serial.print(128);
	Serial.print(128);
	Serial.print(0);
	Serial.print(0);
	Serial.print("\n");
}


void GCSpy::debugKeyboard()
{
	byte vals[3];
	for (int j = 0; j < 3; ++j)
	{
		vals[j] = 0;
		for (int i = 0; i < 8; ++i)
		{
			if (rawData[GC_PREFIX + i + 32 + (j * 8)] != 0)
			{
				vals[j] |= (byte)(1 << (7 - i));
			}
		}
	}
	Serial.print("|");
	Serial.print(vals[0], HEX);
	Serial.print("|");
	Serial.print(vals[1], HEX);
	Serial.print("|");
	Serial.println(vals[2], HEX);
}

void GCSpy::writeKeyboard()
{
	byte vals[3];
	for (int j = 0; j < 3; ++j)
	{
		vals[j] = 0;
		for (int i = 0; i < 8; ++i)
		{
			if (rawData[GC_PREFIX + i + 32 + (j * 8)] != 0)
			{
				vals[j] |= (byte)(1 << (7 - i));
			}
		}
		if (vals[j] == 10)
			vals[j] = 11;
	}
	
	Serial.write(vals[0]);
	Serial.write(vals[1]);
	Serial.write(vals[2]);
	Serial.write(SPLIT);
}

#elif defined(ARDUINO_AVR_UNO) || defined(ARDUINO_AVR_NANO) || defined(ARDUINO_AVR_NANO_EVERY) || defined(ARDUINO_AVR_LARDU_328E)

bool isLuigisMansion = false;

void GCSpy::loop() {
	if (!seenGC2N64) {
		noInterrupts();
		readBits = GC_PREFIX + GC_BITCOUNT;
		updateState();
		interrupts();

		if (checkPrefixGC() || checkPrefixLuigisMansion() || checkPrefixSNES2GC())
		{
#if !defined(DEBUG)
			sendRawData(rawData, GC_PREFIX, isLuigisMansion ? GC_BITCOUNT - 8 : GC_BITCOUNT);
#else
			sendRawDataDebug(rawData, 0, GC_BITCOUNT + GC_PREFIX);
#endif
		}
		else if (checkPrefixKeyboard())
		{
#if !defined(DEBUG)
			writeKeyboard();
#else
			//sendRawData(rawData, GC_PREFIX, GC_BITCOUNT);
			debugKeyboard();
#endif			
		}
		else if (checkPrefixGBA()) {
#if !defined(DEBUG)
			writeSerial();
#else
			debugSerial();
#endif
		}
		else if (checkBothGCPrefixOnRaphnet()) {
			// Sets seenGC2N64RaphnetAdapter to true
		}
		else
		{
#if defined(DEBUG)
			sendRawDataDebug(rawData, 0, GC_BITCOUNT + GC_PREFIX);
#endif
		}
	}
	else {
		noInterrupts();
		readBits = 34 + GC_PREFIX + GC_BITCOUNT;
		updateState();
		interrupts();
		if (checkBothGCPrefixOnRaphnet()) {
			sendRawData(rawData, 34 + GC_PREFIX, GC_BITCOUNT);
		}
		else if (checkPrefixGC()) {
			sendRawData(rawData, GC_PREFIX, GC_BITCOUNT);
		}
		else if (checkPrefixGBA()) {
#if !defined(DEBUG)
			writeSerial();
#else
			debugSerial();
#endif
		}
	}
	T_DELAY(5);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Performs a read cycle from one of Nintendo's one-wire interface based controllers.
void GCSpy::updateState() {
	unsigned char *rawDataPtr = rawData;

read_loop:

	// Wait for the line to go high then low.
	WAIT_FALLING_EDGE(GC_PIN);

	// Wait ~2us between line reads
	asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS);

	// Read a bit from the line and store as a byte in "rawData"
	*rawDataPtr = PIN_READ(GC_PIN);
	++rawDataPtr;
	if (--readBits == 0) return;

	goto read_loop;
}


void GCSpy::writeSerial() {
	Serial.write(ZERO);
	Serial.write(ZERO);
	Serial.write(ZERO);
	Serial.write(rawData[21] ? ONE : ZERO);
	Serial.write(ZERO);
	Serial.write(ZERO);
	Serial.write(rawData[23] ? ONE : ZERO);
	Serial.write(rawData[24] ? ONE : ZERO);
	Serial.write(ZERO);
	Serial.write(rawData[31] ? ONE : ZERO);
	Serial.write(rawData[32] ? ONE : ZERO);
	Serial.write(rawData[22] ? ONE : ZERO);
	Serial.write(rawData[18] ? ONE : ZERO);
	Serial.write(rawData[17] ? ONE : ZERO);
	Serial.write(rawData[20] ? ONE : ZERO);
	Serial.write(rawData[19] ? ONE : ZERO);
	Serial.write(ONE);
	for (int i = 0; i < 7; ++i)
		Serial.write(ZERO);
	Serial.write(ONE);
	for (int i = 0; i < 7; ++i)
		Serial.write(ZERO);
	Serial.write(ONE);
	for (int i = 0; i < 7; ++i)
		Serial.write(ZERO);
	Serial.write(ONE);
	for (int i = 0; i < 7; ++i)
		Serial.write(ZERO);
	for (int i = 0; i < 8; ++i)
		Serial.write(ZERO);
	for (int i = 0; i < 8; ++i)
		Serial.write(ZERO);
	Serial.write(SPLIT);
}

void GCSpy::debugSerial() {
	Serial.print("0");
	Serial.print("0");
	Serial.print("0");
	Serial.print(rawData[21] ? "t" : "0");
	Serial.print("0");
	Serial.print("0");
	Serial.print(rawData[23] ? "b" : "0");
	Serial.print(rawData[24] ? "a" : "0");
	Serial.print("0");
	Serial.print(rawData[31] ? "L" : "0");
	Serial.print(rawData[32] ? "R" : "0");
	Serial.print(rawData[22] ? "s" : "0");
	Serial.print(rawData[18] ? "u" : "0");
	Serial.print(rawData[17] ? "d" : "0");
	Serial.print(rawData[20] ? "l" : "0");
	Serial.print(rawData[19] ? "r" : "0");
	Serial.print(128);
	Serial.print(128);
	Serial.print(128);
	Serial.print(128);
	Serial.print(0);
	Serial.print(0);
	Serial.print("\n");
}

inline bool GCSpy::checkPrefixGBA()
{
	if (rawData[0] != 0) return false; // 0
	if (rawData[1] != 0) return false; // 0
	if (rawData[2] != 0) return false; // 0
	if (rawData[3] == 0) return false; // 1
	if (rawData[4] != 0) return false; // 0
	if (rawData[5] == 0) return false; // 1
	if (rawData[6] != 0) return false; // 0
	if (rawData[7] != 0) return false; // 0
	if (rawData[8] == 0) return false; // 1
	if (rawData[9] != 0) return false; // 0
	if (rawData[10] != 0) return false; // 0
	if (rawData[11] == 0) return false; // 1
	seenGC2N64 = false;
	return true;
}

inline bool GCSpy::checkPrefixSNES2GC()
{
	if (rawData[0] != 0) return false; // 0
	if (rawData[1] == 0) return false;  // 1
	if (rawData[2] != 0) return false;  // 0
	if (rawData[3] != 0) return false;  // 0
	if (rawData[4] != 0) return false;  // 0
	if (rawData[5] != 0) return false;  // 0
	if (rawData[6] != 0) return false;  // 0
	if (rawData[7] != 0) return false;  // 0
	if (rawData[8] != 0) return false;  // 0
	if (rawData[9] != 0) return false;  // 0
	if (rawData[10] != 0) return false;  // 0
	if (rawData[11] != 0) return false;  // 0
	if (rawData[12] != 0) return false;  // 0
	if (rawData[13] != 0) return false;  // 0
	if (rawData[14] == 0) return false;  // 1
	if (rawData[15] == 0) return false;  // 1
	if (rawData[16] != 0) return false;  // 0
	if (rawData[17] != 0) return false;  // 0
	if (rawData[18] != 0) return false;  // 0
	if (rawData[19] != 0) return false;  // 0
	if (rawData[20] != 0) return false;  // 0
	if (rawData[21] != 0) return false;  // 0
	if (rawData[22] != 0) return false; // 0
	if (rawData[23] != 0) return false;  // 0
	
	return true;
}

inline bool GCSpy::checkPrefixKeyboard()
{
	if (rawData[0] != 0) return false; // 0
	if(rawData[1] == 0) return false;  // 1
	if(rawData[2] != 0) return false;  // 0
	if(rawData[3] == 0) return false;  // 1
	if(rawData[4] != 0) return false;  // 0
	if(rawData[5] == 0) return false;  // 1
	if(rawData[6] != 0) return false;  // 0
	if(rawData[7] != 0) return false;  // 0
	if(rawData[8] != 0) return false;  // 0
	if(rawData[9] != 0) return false;  // 0
	if(rawData[10] != 0) return false;  // 0
	if(rawData[11] != 0) return false;  // 0
	if(rawData[12] != 0) return false;  // 0
	if(rawData[13] != 0) return false;  // 0
	if(rawData[14] != 0) return false;  // 0
	if(rawData[15] != 0) return false;  // 0
	if(rawData[16] != 0) return false;  // 0
	if(rawData[17] != 0) return false;  // 0
	if(rawData[18] != 0) return false;  // 0
	if(rawData[19] != 0) return false;  // 0
	if(rawData[20] != 0) return false;  // 0
	if(rawData[21] != 0) return false;  // 0
	if (rawData[22] != 0) return false; // 0
	if(rawData[23] != 0) return false;  // 0
	if(rawData[24] == 0) return false;  // 1
	seenGC2N64 = false;
	return true;

}

inline bool GCSpy::checkPrefixGC()
{
	
	if (rawData[0] != 0) return false; // 0
	if (rawData[1] == 0) return false; // 1
	if (rawData[2] != 0) return false; // 0
	if (rawData[3] != 0) return false; // 0
	if (rawData[4] != 0) return false; // 0
	if (rawData[5] != 0) return false; // 0
	if (rawData[6] != 0) return false; // 0
	if (rawData[7] != 0) return false; // 0
	if (rawData[8] != 0) return false; // 0
	if (rawData[9] != 0) return false; // 0
	if (rawData[10] != 0) return false; // 0
	if (rawData[11] != 0) return false; // 0
	if (rawData[12] != 0) return false; // 0
	if (rawData[13] != 0) return false; // 0
	if (rawData[14] == 0) return false; // 1
	if (rawData[15] == 0) return false; // 1
	if (rawData[16] != 0) return false; // 0
	if (rawData[17] != 0) return false; // 0
	if (rawData[18] != 0) return false; // 0
	//if (rawData[19] != 0) return false; // 0 or 1
	if (rawData[20] != 0) return false; // 0
	if (rawData[21] != 0) return false; // 0
	//if (rawData[22] != 0) return false; // 0 or 1
	if (rawData[23] != 0) return false; // 0
	if (rawData[24] == 0) return false; // 1
	seenGC2N64 = false;
	return true;
}

inline bool GCSpy::checkPrefixLuigisMansion()
{
	isLuigisMansion = false;
	if (rawData[0] != 0) return false; // 0
	if(rawData[1] == 0) return false;  // 1
	if(rawData[2] != 0) return false;  // 0
	if(rawData[3] != 0) return false;  // 0
	if(rawData[4] != 0) return false;  // 0
	if(rawData[5] != 0) return false;  // 0
	if(rawData[6] != 0) return false;  // 0
	if(rawData[7] != 0) return false;  // 0
	if(rawData[8] != 0) return false;  // 0
	if(rawData[9] != 0) return false;  // 0
	if(rawData[10] != 0) return false;  // 0
	if(rawData[11] != 0) return false;  // 0
	if(rawData[12] != 0) return false;  // 0
	if(rawData[13] != 0) return false;  // 0
	if(rawData[14] != 0) return false;  // 0
	if(rawData[15] != 0) return false;  // 0
	if(rawData[16] != 0) return false;  // 0
	if(rawData[17] != 0) return false;  // 0
	if(rawData[18] != 0) return false;  // 0
	if(rawData[19] != 0) return false;  // 0
	if(rawData[20] != 0) return false;  // 0
	if(rawData[21] != 0) return false;  // 0
	//if (rawData[22] != 0) return false; // 0 or 1
	if(rawData[23] != 0) return false;  // 0
	if(rawData[24] == 0) return false;  // 1
	seenGC2N64 = false;
	isLuigisMansion = true;
	return true;
}

inline bool GCSpy::checkBothGCPrefixOnRaphnet()
{
	if (rawData[0] != 0) return false; // 0
	if (rawData[1] != 0) return false; // 0
	if (rawData[2] != 0) return false; // 0
	if (rawData[3] != 0) return false; // 0
	if (rawData[4] != 0) return false; // 0
	if (rawData[5] != 0) return false; // 0
	if (rawData[6] != 0) return false; // 0
	if (rawData[7] != 0) return false; // 0
	if (rawData[8] == 0) return false; // 1
	if (rawData[9] != 0) return false; // 0
	if (rawData[10] != 0) return false; // 0
	if (rawData[11] != 0) return false; // 0
	if (rawData[12] != 0) return false; // 0
	if (rawData[13] == 0) return false; // 1
	if (rawData[14] != 0) return false; // 0
	if (rawData[15] != 0) return false; // 0
	if (rawData[16] == 0) return false; // 1
	if (rawData[17] != 0) return false; // 0
	if (rawData[18] != 0) return false; // 0
	if (rawData[19] != 0) return false; // 0
	if (rawData[20] != 0) return false; // 0
	if (rawData[21] != 0) return false; // 0
	if (rawData[22] != 0) return false; // 0
	if (rawData[23] != 0) return false; // 0
	if (rawData[24] != 0) return false; // 0
	if (rawData[25] != 0) return false; // 0
	//if (rawData[26] != 0) return false; // 0 or 1
	if (rawData[27] == 0) return false; // 1
	if (rawData[28] != 0) return false; // 0
	if (rawData[29] != 0) return false; // 0
	if (rawData[30] != 0) return false; // 0
	if (rawData[31] == 0) return false; // 1
	if (rawData[32] == 0) return false; // 1
	if (rawData[33] != 0) return false; // 0
	if (rawData[34] != 0) return false; // 0
	if (rawData[35] == 0) return false; // 1
	if (rawData[36] != 0) return false; // 0
	if (rawData[37] != 0) return false; // 0
	if (rawData[38] != 0) return false; // 0
	if (rawData[39] != 0) return false; // 0
	if (rawData[40] != 0) return false; // 0
	if (rawData[41] != 0) return false; // 0
	if (rawData[42] != 0) return false; // 0
	if (rawData[43] != 0) return false; // 0
	if (rawData[44] != 0) return false; // 0
	if (rawData[45] != 0) return false; // 0
	if (rawData[46] != 0) return false; // 0
	if (rawData[47] != 0) return false; // 0
	if (rawData[48] == 0) return false; // 1
	if (rawData[49] == 0) return false; // 1
	if (rawData[50] != 0) return false; // 0
	if (rawData[51] != 0) return false; // 0
	if (rawData[52] != 0) return false; // 0
	if (rawData[53] != 0) return false; // 0
	if (rawData[54] != 0) return false; // 0
	if (rawData[55] != 0) return false; // 0
	if (rawData[56] != 0) return false; // 0
	if (rawData[57] != 0) return false; // 0
	if (rawData[58] == 0) return false; // 1
	seenGC2N64 = true;
	return true;
}

void GCSpy::debugKeyboard()
{
	byte vals[3];
	for (int j = 0; j < 3; ++j)
	{
		vals[j] = 0;
		for (int i = 0; i < 8; ++i)
		{
			if (rawData[GC_PREFIX + i + 32 + (j * 8)] != 0)
			{
				vals[j] |= (byte)(1 << (7 - i));
			}
		}
	}
	Serial.print("|");
	Serial.print(vals[0], HEX);
	Serial.print("|");
	Serial.print(vals[1], HEX);
	Serial.print("|");
	Serial.println(vals[2], HEX);
}

void GCSpy::writeKeyboard()
{
	byte vals[3];
	for (int j = 0; j < 3; ++j)
	{
		vals[j] = 0;
		for (int i = 0; i < 8; ++i)
		{
			if (rawData[GC_PREFIX + i + 32 + (j * 8)] != 0)
			{
				vals[j] |= (byte)(1 << (7 - i));
			}
		}
		if (vals[j] == 10)
			vals[j] = 11;
	}
	
	Serial.write(vals[0]);
	Serial.write(vals[1]);
	Serial.write(vals[2]);
	Serial.write(SPLIT);
}

#else

void GCSpy::loop() {}
void GCSpy::writeSerial() {}
void GCSpy::debugSerial() {}
void GCSpy::updateState() {}

#endif

const char* GCSpy::startupMsg()
{
	return "GC";
}