//
// CDTVWired.cpp
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

#include "CDTVWired.h"

#if (defined(TP_TIMERONE) && !(defined(__arm__) && defined(CORE_TEENSY))) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
#include <TimerOne.h>
#define PRDT_PIN 2
#else
#include <elapsedMillis.h>
#define PRDT_PIN 0
#endif
#define BUFFER_SIZE 45

#define WAITING_FOR_SYNC_HIGH 1
#define WAITING_FOR_SYNC_LOW  2
#define SYNCED                3
#define WAITING_FOR_START     4

struct CDTV_packet
{
	byte rawData[25];
};

static volatile CDTV_packet* buffer[BUFFER_SIZE];
static uint8_t head, tail;
static volatile CDTV_packet* currentReadPacket = NULL;

static volatile CDTV_packet* currentWritePacket = NULL;

#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
static volatile unsigned long diff;
#else
static volatile uint32_t timestamp;
#endif

static volatile unsigned int count = 0, state = WAITING_FOR_SYNC_HIGH;

void PRDTStateChanged()
{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	diff = TCNT1 >> 1;
#else	uint32_t diff = micros() - timestamp;
#endif
	if (state == WAITING_FOR_SYNC_HIGH) {
		if (diff > 1000) {
			state = WAITING_FOR_SYNC_LOW;
		}
	}
	else if (state == WAITING_FOR_SYNC_LOW) {
		if (diff > 1000)
		{
			state = SYNCED;
			if (currentWritePacket == NULL)
				currentWritePacket = new CDTV_packet();
			count = -1;
		}
	}
	else if (state == SYNCED) {
		//Serial.println("SYNCED");
		if (count % 2 == 0) {
			if (diff > 400) {
				currentWritePacket->rawData[count / 2] = 1;
			}
			else
			{
				currentWritePacket->rawData[count / 2] = 0;
			}
		}
		count++;
		if ((count == 48 && currentWritePacket->rawData[0] == 0) || (count == 38 && currentWritePacket->rawData[0] == 1)) {
			volatile CDTV_packet* donePacket = currentWritePacket;
			state = WAITING_FOR_START;
			count = 0;
			currentWritePacket = NULL;
			uint8_t i = head + 1;
			if (i >= BUFFER_SIZE) i = 0;
			if (i != tail) {
				buffer[i] = donePacket;
				head = i;
			}
		}
	}
	else if (state == WAITING_FOR_START)
	{
		//Serial.println("WAITING_FOR_START");
		if (count == 2)
		{
			if (currentWritePacket == NULL)
				currentWritePacket = new CDTV_packet();
			count = -1;
			state = SYNCED;
		}
		else
			count++;
	}
	
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	TCNT1 = 0;
#else
	timestamp = micros();
#endif
}

void CDTVWiredSpy::setup()
{
	printFirmwareInfo();

	pinMode(PRDT_PIN, INPUT_PULLUP);
	attachInterrupt(digitalPinToInterrupt(PRDT_PIN), PRDTStateChanged, CHANGE);
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	Timer1.initialize(10000);
	Timer1.stop();
	Timer1.restart();
	Timer1.detachInterrupt();
#else
	timestamp = micros();
#endif

}

void CDTVWiredSpy::loop()
{
	uint8_t i = tail;
	if (i != head)
	{
		i++;
		if (i >= BUFFER_SIZE) i = 0;
		currentReadPacket = buffer[i];
		tail = i;

#ifdef DEBUG
		for (int i = 0; i < 24; ++i)
			Serial.print(currentReadPacket->rawData[i] == 0 ? "0" : "1");

		Serial.print("|");
		byte x = 0;
		for (int i = 3; i < 11; ++i)
		{
			if (currentReadPacket->rawData[i] == 1)
				x |= (1 << (7 - (i - 3)));
		}
		Serial.print(x);
		Serial.print("|");
		byte y = 0;
		for (int i = 11; i < 19; ++i)
		{
			if (currentReadPacket->rawData[i] == 1)
				y |= (1 << (7 - (i - 11)));
		}
		Serial.print(y);
		Serial.print("\n");

#else
		int checksum = 0;
		for (int i = 0; i < 24; ++i)
		{
			checksum += currentReadPacket->rawData[i] == 0 ? 0 : 1;
			Serial.write(currentReadPacket->rawData[i] == 0 ? 0 : 1);
		}
		Serial.write((checksum & 0x0F) << 4);
		Serial.write((checksum & 0xF0));
		Serial.write("\n");
#endif
		delete currentReadPacket;
	}
}

void CDTVWiredSpy::writeSerial() {}
void CDTVWiredSpy::debugSerial() {}
void CDTVWiredSpy::updateState() {}

#else
void CDTVWiredSpy::loop() {}
void CDTVWiredSpy::setup() {}
void CDTVWiredSpy::writeSerial() {}
void CDTVWiredSpy::debugSerial() {}
void CDTVWiredSpy::updateState() {}
#endif

const char* CDTVWiredSpy::startupMsg()
{
	return "CDTV Wired";
}