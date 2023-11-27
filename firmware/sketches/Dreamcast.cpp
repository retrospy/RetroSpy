//
// Dreamcast.cpp
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

#include "Dreamcast.h"




#if (defined(__arm__) && defined(CORE_TEENSY) && (defined (ARDUINO_TEENSY35) || defined(ARDUINO_TEENSY40) || defined(ARDUINO_TEENSY41)))
#if defined (ARDUINO_TEENSY35)
#define DETECT_FALLING_EDGE rawData[byteCount] = (GPIOD_PDIR & 0x3); do { prevPin = rawData[byteCount]; rawData[byteCount] = (GPIOD_PDIR & 0x3); } while( rawData[byteCount] >= prevPin);
#elif defined(ARDUINO_TEENSY40) || defined(ARDUINO_TEENSY41)
#define DETECT_FALLING_EDGE rawData[byteCount] = (GPIO6_PSR >> 2) & 0x03; do { prevPin = rawData[byteCount]; rawData[byteCount] = (GPIO6_PSR >> 2) & 0x03; } while( rawData[byteCount] >= prevPin);
#endif

void DreamcastSpy::setup() {
	pinMode(2, INPUT);
	pinMode(14, INPUT);
	pinMode(0, INPUT);
	pinMode(1, INPUT);
	
	p = &rawData[6];
}

FASTRUN void DreamcastSpy::loop()
{
	updateState();

#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif
}

FASTRUN void DreamcastSpy::writeSerial() {
	int j = 0;
	byte numFrames = 0;
	for (int i = 0; i < 4; ++i)
	{
		numFrames |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
		numFrames |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
		j += 2;
	}
	j += 16;
	byte dcCommand = 0;
	for (int i = 0; i < 4; ++i)
	{
		dcCommand |= (byte)(((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
		dcCommand |= (byte)(((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
		j += 2;
	}
	if (dcCommand == 8 && numFrames >= 1)
	{
		uint controllerType = 0;
		for (int i = 0; i < 2; i++)
		{
			for (int k = 0; k < 4; ++k)
			{
				controllerType |= (uint)(((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (k * 2) + (i * 8)));
				controllerType |= (uint)(((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8)));
				j += 2;
			}
		}

		if ((controllerType == 1 && numFrames == 3) || (controllerType == 0x200 && numFrames == 6) || (controllerType == 0x40 && numFrames == 3))
		{
			rawData[byteCount++] = '\n';
			Serial.write(p, byteCount - 6);
		}
	}
}

FASTRUN void DreamcastSpy::debugSerial() {
	int j = 0;

	Serial.print(byteCount - 6);
	Serial.print("|");

	byte numFrames = 0;
	for (int i = 0; i < 4; ++i)
	{
		numFrames |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
		numFrames |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
		j += 2;
	}
	Serial.print(numFrames);

	for (int i = 8; i < 16; i++)
	{
		if (j % 8 == 0 && j != 0)
			Serial.print("|");
		Serial.print((p[j] & 0x02) != 0 ? "1" : "0");
		Serial.print((p[j + 1] & 0x01) != 0 ? "1" : "0");
		j += 2;
	}

	Serial.print("|");
	byte dcCommand = 0;
	for (int i = 0; i < 4; ++i)
	{
		dcCommand |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
		dcCommand |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
		j += 2;
	}
	Serial.print(dcCommand);

	Serial.print("|");
	unsigned int controllerType = 0;
	for (int i = 0; i < 2; i++)
	{
		for (int k = 0; k < 4; ++k)
		{
			controllerType |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (k * 2) + (i * 8));
			controllerType |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8));
			j += 2;
		}
	}
	Serial.print(controllerType);

	j += 16;

	if (dcCommand == 8 && controllerType == 0x40)
	{
		byte keycode[6];

		keycode[1] = 0;
		for (int i = 0; i < 4; ++i)
		{
			keycode[1] |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			keycode[1] |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}

		keycode[0] = 0;
		for (int i = 0; i < 4; ++i)
		{
			keycode[0] |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			keycode[0] |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}

		Serial.print("|");
		for (int i = 0; i < 4; i++)
		{
			Serial.print((p[j] & 0x02) != 0 ? "1" : "0");
			Serial.print((p[j + 1] & 0x01) != 0 ? "1" : "0");
			j += 2;
		}

		Serial.print("|");
		for (int i = 0; i < 4; i++)
		{
			Serial.print((p[j] & 0x02) != 0 ? "1" : "0");
			Serial.print((p[j + 1] & 0x01) != 0 ? "1" : "0");
			j += 2;
		}

		keycode[5] = 0;
		for (int i = 0; i < 4; ++i)
		{
			keycode[5] |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			keycode[5] |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}

		keycode[4] = 0;
		for (int i = 0; i < 4; ++i)
		{
			keycode[4] |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			keycode[4] |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}

		keycode[3] = 0;
		for (int i = 0; i < 4; ++i)
		{
			keycode[3] |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			keycode[3] |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}

		keycode[2] = 0;
		for (int i = 0; i < 4; ++i)
		{
			keycode[2] |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			keycode[2] |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}

		for (int i = 0; i < 6; ++i)
		{
			Serial.print("|");
			Serial.print(keycode[i], HEX);
		}
	}
	else if (dcCommand == 8 && controllerType == 0x200)
	{
		j += 24;
		Serial.print("|");
		for (int i = 0; i < 4; i++)
		{
			Serial.print((p[j] & 0x02) != 0 ? "1" : "0");
			Serial.print((p[j + 1] & 0x01) != 0 ? "1" : "0");
			j += 2;
		}

		Serial.print("|");
		unsigned int axis1 = 0;
		for (int i = 1; i >= 0; --i)
		{
			for (int k = 0; k < 4; ++k)
			{
				axis1 |= ((p[j] & 0x02) != 0 ? 1 : 0) << ((7 - k * 2) + (i * 8));
				axis1 |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8));
				j += 2;
			}
		}
		Serial.print(axis1);

		Serial.print("|");
		unsigned int axis2 = 0;
		for (int i = 1; i >= 0; --i)
		{
			for (int k = 0; k < 4; ++k)
			{
				axis2 |= ((p[j] & 0x02) != 0 ? 1 : 0) << ((7 - k * 2) + (i * 8));
				axis2 |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8));
				j += 2;
			}
		}
		Serial.print(axis2);

		j += 16;

		Serial.print("|");
		unsigned int axis3 = 0;
		for (int i = 1; i >= 0; --i)
		{
			for (int k = 0; k < 4; ++k)
			{
				axis3 |= ((p[j] & 0x02) != 0 ? 1 : 0) << ((7 - k * 2) + (i * 8));
				axis3 |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8));
				j += 2;
			}
		}
		Serial.print(axis3);
	}
	else if (dcCommand == 8 && controllerType == 1)
	{
		Serial.print("|");
		byte ltrigger = 0;
		for (int i = 0; i < 4; ++i)
		{
			ltrigger |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			ltrigger |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}
		Serial.print(ltrigger);

		Serial.print("|");
		byte rtrigger = 0;
		for (int i = 0; i < 4; ++i)
		{
			rtrigger |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			rtrigger |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}
		Serial.print(rtrigger);

		for (int i = 0; i < 8; i++)
		{
			if (j % 8 == 0 && j != 0)
				Serial.print("|");
			Serial.print((p[j] & 0x02) != 0 ? "1" : "0");
			Serial.print((p[j + 1] & 0x01) != 0 ? "1" : "0");
			j += 2;
		}

		j += 16;

		Serial.print("|");
		byte joyy = 0;
		for (int i = 0; i < 4; ++i)
		{
			joyy |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			joyy |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}
		Serial.print(joyy);

		Serial.print("|");
		byte joyx = 0;
		for (int i = 0; i < 4; ++i)
		{
			joyx |= ((p[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2));
			joyx |= ((p[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2));
			j += 2;
		}
		Serial.print(joyx);
	}
	Serial.print("\n");
}

FASTRUN void DreamcastSpy::updateState() {
	byte prevPin;
	
	//start_state:
state1:
	interrupts();
	byteCount = 0;
	DETECT_FALLING_EDGE
		if (prevPin != 0x3 || rawData[byteCount] != 0x2)   // Starts with both pins High and pin 1 drops
			goto state1;
	++byteCount;

	//state2:
	DETECT_FALLING_EDGE
		if (prevPin != 0x2 || rawData[byteCount] != 0x0)  // Pin 5 now drops, pin 1 stays low
			goto state1;
	++byteCount;

state3:

	DETECT_FALLING_EDGE
		if (prevPin != 0x2 || rawData[byteCount] != 0x0)  // Pin 5 drops 3 more times, pin 1 says low
			goto state1;
		else if (++byteCount != 5)
			goto state3;

	//state4:
	DETECT_FALLING_EDGE
		if (rawData[byteCount] != 0x01 || prevPin != 0x3) // Pin 5 drops 1 more times, this time Pin 1 is high
			goto state1;
	++byteCount;

	noInterrupts();
state5:                             // Phase 1
	DETECT_FALLING_EDGE
		if (prevPin == 0x3 && rawData[byteCount] == 0x1)
			goto state7;
		else if (prevPin == 0x02 || prevPin == 0x0)
			goto state1;
	++byteCount;

	//state6:                           // Phase 2
	DETECT_FALLING_EDGE
		if (prevPin == 0x01 || prevPin == 0x0)
			goto state1;
	++byteCount;
	goto state5;

state7:
	interrupts();
}
#elif defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

#include "MapleBusInterface.hpp"
#include "maple_in.pio.h"
#include "maple_out.pio.h"

static std::shared_ptr<MapleBusInterface> bus;
MaplePacket mPacketIn;

void DreamcastSpy::setup() {
	
	mPacketIn.reservePayload(256);
	
	// Create the bus for client-mode operation
	bus = create_maple_bus(2, P1_DIR_PIN, DIR_OUT_HIGH);
}

static int len;
FASTRUN void DreamcastSpy::loop1()
{
	if (sendRequest)
	{
		len = rawData[0];
		memcpy(sendData, &rawData[1], len);
		sendRequest = false;

#ifdef DEBUG
		debugSerial();
#else
		writeSerial();
#endif
	}
}

static const uint64_t READ_TIMEOUT_US = 1000000;

FASTRUN void DreamcastSpy::loop()
{
	while (sendRequest)
	{
	}
	
	MapleBusInterface::Status status = bus->processEvents(micros());
	switch (status.phase)
	{
	case MapleBusInterface::Phase::WAITING_FOR_READ_START: // Fall through
	case MapleBusInterface::Phase::READ_IN_PROGRESS:
		{
			// Nothing to do (waiting for current process to complete)
		}
		break;

	case MapleBusInterface::Phase::READ_COMPLETE:
		{
			mPacketIn.set(status.readBuffer, status.readBufferLen);
			if (mPacketIn.frame.command == 8)
			{
				rawData[0] = mPacketIn.payload.size() * 4;
				for (int i = 0; i < mPacketIn.payload.size(); ++i)
				{
					rawData[(i * 4 + 0) + 1] = ((mPacketIn.payload[i] & 0xFF000000) >> 24);
					rawData[(i * 4 + 1) + 1] = ((mPacketIn.payload[i] & 0x00FF0000) >> 16);
					rawData[(i * 4 + 2) + 1] = ((mPacketIn.payload[i] & 0x0000FF00) >> 8);
					rawData[(i * 4 + 3) + 1] = ((mPacketIn.payload[i] & 0x000000FF) >> 0);
				}
				sendRequest = true;
			}
		}
		break;
	default:
		{
			(void)bus->startRead(READ_TIMEOUT_US);
		}
		break;
	}
}

FASTRUN void DreamcastSpy::writeSerial()
{
	for (int i = 0; i < len; i++)
	{
		Serial.write((sendData[i] & 0x0F) << 4);
		Serial.write(sendData[i] & 0xF0);
	}
	Serial.write(SPLIT);
}

FASTRUN void DreamcastSpy::debugSerial() {
	
	uint16_t controllerType = (sendData[2] << 8) | sendData[3];

	if (controllerType == 0x01 && len == 12)
	{
		Serial.print(sendData[7]);
		Serial.print("|");
		Serial.print(sendData[6]);
		Serial.print("|");
			
		for (int i = 0; i < 16; ++i)
		{
			Serial.print((sendData[i / 8 + 4] & (1 << (i % 8))) == 0 ? "1" : "0");
		}
			
		Serial.print("|");
		Serial.print(sendData[9]);
		Serial.print("|");
		Serial.println(sendData[8]);
	}
	else if (controllerType == 0x200 && len == 24)
	{
		for (int i = 0; i < 8; ++i)
		{
			Serial.print((sendData[4] & (1 << i)) == 0 ? "1" : "0");
		}
		Serial.print("|");
		Serial.print(sendData[11] << 8 | sendData[10]);
		Serial.print("|");
		Serial.print(sendData[9] << 8 | sendData[8]);
		Serial.print("|");
		Serial.println(sendData[13] << 8 | sendData[12]);
	}
	else if (controllerType == 0x40 && len == 12)
	{
		for (int i = 0; i < 8; ++i)
		{
			Serial.print((sendData[4] & (1 << i)) == 0 ? "1" : "0");
		}
		Serial.print("|");
		Serial.print(sendData[6]);
		Serial.print("|");
		Serial.print(sendData[7]);
		Serial.print("|");
		Serial.print(sendData[8]);
		Serial.print("|");
		Serial.print(sendData[9]);
		Serial.print("|");
		Serial.print(sendData[10]);
		Serial.print("|");
		Serial.println(sendData[11]);
	}
}
FASTRUN void DreamcastSpy::updateState() {

}
#else
void DreamcastSpy::setup() {
}

void DreamcastSpy::loop() {
}

void DreamcastSpy::loop1() {
}


void DreamcastSpy::writeSerial() {
}

void DreamcastSpy::debugSerial() {
}

void DreamcastSpy::updateState() {
}
#endif

const char* DreamcastSpy::startupMsg()
{
	return "Dreamcast";
}