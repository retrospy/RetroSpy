//
// Pippin.cpp
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

// ---------- Uncomment this for a mostly complete general purpose ADB sniffer  --------------
//#define SNIFFER

#include "Pippin.h"

#if defined(TP_TIMERONE) && !(defined(__arm__) && defined(CORE_TEENSY))
#include <TimerOne.h>

#define BUFFER_SIZE 45

#define ADB_PIN 2

#define WAITING_FOR_ATTENTION 0
#define WAITING_FOR_SYNC 1
#define READING_COMMAND_BITS 2
#define WAITING_FOR_CMD_STOP 3
#define WAITING_FOR_DATA_START 4
#define WAITING_DATA_START_BIT 5
#define READING_DATA_BITS 6
#define COULD_BE_DATA_STOP_BIT 7

struct Pippin_packet
{
	byte commandType = 0;
	byte commandAddress = 0;
	byte commandRegister = 0;
	bool commandStop = false;
	bool HasData = false;
	bool dataStart = false;
	byte numBytes = 0;
	byte data[9];
	bool dataStop = false;
	int count = 0;
	int syncTiming = 0;
}
;

static volatile Pippin_packet* buffer[BUFFER_SIZE];
static uint8_t head, tail;
static volatile Pippin_packet* currentReadPacket = NULL;

static volatile Pippin_packet* currentWritePacket = NULL;
static volatile unsigned long diff;
static volatile unsigned int count = 0, state = WAITING_FOR_ATTENTION;
static volatile unsigned char command;
byte lastMouseReportID;

static byte rawData[16][40];

void PippinSpy::loop()
{
	if (state == WAITING_FOR_DATA_START)
	{
		unsigned long _diff = TCNT1 >> 1;
		if (_diff > 260)
		{
			currentWritePacket->HasData = false;
			volatile Pippin_packet* donePacket = currentWritePacket;
			currentWritePacket = NULL;
			state = WAITING_FOR_ATTENTION;
			uint8_t i = head + 1;
			if (i >= BUFFER_SIZE) i = 0;
			if (i != tail) {
				buffer[i] = donePacket;
				head = i;
			}
		}
	}
	else if (state == COULD_BE_DATA_STOP_BIT)
	{
		unsigned long _diff = TCNT1 >> 1;
		if (_diff > 50)
		{
			currentWritePacket->dataStop = true;
			volatile Pippin_packet* donePacket = currentWritePacket;
			currentWritePacket = NULL;
			state = WAITING_FOR_ATTENTION;
			uint8_t i = head + 1;
			if (i >= BUFFER_SIZE) i = 0;
			if (i != tail) {
				buffer[i] = donePacket;
				head = i;
			}
		}
	}

	uint8_t i = tail;
	if (i != head)
	{
		i++;
		if (i >= BUFFER_SIZE) i = 0;
		currentReadPacket = buffer[i];
		tail = i;

#ifdef SNIFFER
		
		Serial.print(currentReadPacket->commandType, HEX);
		Serial.print(" ");
		Serial.print(currentReadPacket->commandAddress, HEX);
		Serial.print(" ");
		Serial.print(currentReadPacket->commandRegister, HEX);
		Serial.print(" ");
		Serial.print(currentReadPacket->syncTiming);
		Serial.print(" ");
		Serial.print(currentReadPacket->commandStop ? "S" : "-");
		Serial.print(" ");
		Serial.print(currentReadPacket->HasData ? "D" : "N");
		Serial.print(" ");
		Serial.print(currentReadPacket->dataStart ? "S" : "-");
		Serial.print(" ");
		Serial.print(currentReadPacket->numBytes, HEX);
		Serial.print(" ");
		Serial.print(currentReadPacket->count);
		Serial.print(" ");
		for (int j = 0; j < currentReadPacket->numBytes; ++j)
		{
			Serial.print(currentReadPacket->data[j], HEX);
			Serial.print(" ");
		}
		Serial.println(currentReadPacket->dataStop ? "S" : "-");
		
#else
		if (initialControllerAddress != initialMouseAddress)
		{
			if (currentReadPacket->HasData && currentReadPacket->numBytes == 4 && currentReadPacket->commandAddress != 0x02) 
			{
				controllerAddress = currentReadPacket->commandAddress;
			
				for (int j = 0; j < 3; ++j)
				{
					for (int k = 0; k < 8; ++k)
						rawData[currentReadPacket->commandAddress][(j * 8) + k] = (currentReadPacket->data[j] & (1 << k)) != 0;
				}
				rawData[currentReadPacket->commandAddress][24] = (currentReadPacket->data[3] & 0b00000001) != 0;
				rawData[currentReadPacket->commandAddress][25] = (currentReadPacket->data[3] & 0b00000010) != 0;
				rawData[currentReadPacket->commandAddress][26] = (currentReadPacket->data[3] & 0b00000100) != 0;
			}
			else if (currentReadPacket->HasData && currentReadPacket->numBytes == 2 && currentReadPacket->commandAddress != 0x02) 
			{
				mouseAddress = currentReadPacket->commandAddress;
				for (int j = 0; j < 2; ++j)
				{
					for (int k = 0; k < 8; ++k)
						rawData[currentReadPacket->commandAddress][(j * 8) + k] = (currentReadPacket->data[j] & (1 << k)) != 0;
				}  
			}
			else if (currentReadPacket->HasData && currentReadPacket->numBytes == 3 && currentReadPacket->commandAddress != 0x02)
			{
				joystickAddress = currentReadPacket->commandAddress;
				for (int j = 0; j < 3; ++j)
				{
					for (int k = 0; k < 8; ++k)
						rawData[currentReadPacket->commandAddress][(j * 8) + k] = (currentReadPacket->data[j] & (1 << k)) != 0;
				}
			}
			else if (currentReadPacket->HasData && currentReadPacket->numBytes == 5)
			{
				tabletAddress = currentReadPacket->commandAddress;
				for (int j = 0; j < 4; ++j)
				{
					for (int k = 0; k < 8; ++k)
						rawData[currentReadPacket->commandAddress][(j * 8) + k] = (currentReadPacket->data[j] & (1 << k)) != 0;
				}
				rawData[currentReadPacket->commandAddress][32] = 1;
				for (int j = 0; j < 7; ++j)
					rawData[currentReadPacket->commandAddress][33 + j] = 0;
			}
			else if (!currentReadPacket->HasData && currentReadPacket->numBytes == 0 && 
				(currentReadPacket->commandAddress == mouseAddress || currentReadPacket->commandAddress == controllerAddress))
			{
				for (int i = 0; i < 7; ++i)
					rawData[currentReadPacket->commandAddress][i] = 0;
				for (int i = 8; i < 15; ++i)
					rawData[currentReadPacket->commandAddress][i] = 0;
			}
			else if (!currentReadPacket->HasData && currentReadPacket->numBytes == 0 && currentReadPacket->commandAddress == tabletAddress)
			{
				rawData[currentReadPacket->commandAddress][32] = 0;
			}
			else if (currentReadPacket->HasData && currentReadPacket->numBytes == 2 && currentReadPacket->commandAddress == 0x02) 
			{
      
				if (currentReadPacket->data[0] == 0x7F && currentReadPacket->data[1] == 0x7F)
				{
					rawData[currentReadPacket->commandAddress][currentReadPacket->data[0] / 8] |= (1 << (currentReadPacket->data[0] % 8));
				}
				else if (currentReadPacket->data[0] == 0xFF && currentReadPacket->data[1] == 0xFF)
				{
					rawData[currentReadPacket->commandAddress][0x7F / 8] &= ~(1 << (0x7F % 8));
				}
				else
				{
					for (int i = 0; i < 2; ++i)
					{
						if ((currentReadPacket->data[i] & 0b10000000) != 0 && currentReadPacket->data[i] != 0xFF)
						{
							rawData[currentReadPacket->commandAddress][(currentReadPacket->data[i] & 0b01111111) / 8] &= ~(1 << ((currentReadPacket->data[i] & 0b01111111) % 8));
						}
						else if (currentReadPacket->data[i] != 0xFF)
						{ 
							rawData[currentReadPacket->commandAddress][currentReadPacket->data[i] / 8] |= (1 << (currentReadPacket->data[i] % 8));
						}
					}
				}
			}
		}
		else
		{
			Serial.print(currentReadPacket->commandType, HEX);
			Serial.print(" ");
			Serial.print(currentReadPacket->commandAddress, HEX);
			Serial.print(" ");
			Serial.print(currentReadPacket->commandRegister, HEX);
			Serial.print(" ");
			Serial.print(currentReadPacket->syncTiming);
			Serial.print(" ");
			Serial.print(currentReadPacket->commandStop ? "S" : "-");
			Serial.print(" ");
			Serial.print(currentReadPacket->HasData ? "D" : "N");
			Serial.print(" ");
			Serial.print(currentReadPacket->dataStart ? "S" : "-");
			Serial.print(" ");
			Serial.print(currentReadPacket->numBytes, HEX);
			Serial.print(" ");
			Serial.print(currentReadPacket->count);
			Serial.print(" ");
			for (int j = 0; j < currentReadPacket->numBytes; ++j)
			{
				Serial.print(currentReadPacket->data[j], HEX);
				Serial.print(" ");
			}
			Serial.println(currentReadPacket->dataStop ? "S" : "-");			
		}
#ifdef DEBUG
		if (currentReadPacket->commandAddress == tabletAddress)
		{
			if (currentReadPacket->HasData)
			{
				Serial.print((currentReadPacket->data[2] & 0b10000000) != 0);
				Serial.print("|");
				Serial.print(((currentReadPacket->data[0] & 0b00011111)*32) + (currentReadPacket->data[1] >> 3));
				Serial.print("|");
				Serial.print(((currentReadPacket->data[2] & 0b00011111)*128) + (currentReadPacket->data[3] >> 1));
				Serial.print("\n");	
			}
			else
			{
				Serial.print("0");
				Serial.print("|");
				Serial.print(-1);
				Serial.print("|");
				Serial.print(-1);
				Serial.print("\n");				
			}
			
		}
		else if (currentReadPacket->commandAddress != 0x02)
		{
			Serial.print((currentReadPacket->commandAddress & 0x0F) << 4);
			Serial.print("|");
			Serial.print(currentReadPacket->commandAddress & 0xF0);
			Serial.print("|");
			for (int j = 0; j < 27; ++j)
				Serial.print(rawData[currentReadPacket->commandAddress][j] == 0 ? "0" : "1");
			Serial.print("\n");
		}
		else
		{
			Serial.print((currentReadPacket->commandAddress & 0x0F) << 4);
			Serial.print("|");
			Serial.print(currentReadPacket->commandAddress & 0xF0);
			Serial.print("|");
			for (int i = 0; i < 16; ++i)
			{
				Serial.print(rawData[currentReadPacket->commandAddress][i], HEX);
				Serial.print(" ");
			}
			Serial.print("\n");        
		}
#else
		if (initialControllerAddress != initialMouseAddress)
		{
			Serial.write((currentReadPacket->commandAddress & 0x0F) << 4);
			Serial.write(currentReadPacket->commandAddress & 0xF0);
			if (currentReadPacket->commandAddress == controllerAddress || (currentReadPacket->commandAddress == 0x02 && lastMouseReportID == controllerAddress))
			{
				lastMouseReportID = controllerAddress;
				for (int j = 0; j < 27; ++j)
					Serial.write(rawData[controllerAddress][j] == 0 ? 0 : 1);
			}
			else if (currentReadPacket->commandAddress == mouseAddress || (currentReadPacket->commandAddress == 0x02 && lastMouseReportID == mouseAddress))
			{
				lastMouseReportID = mouseAddress;
				for (int j = 0; j < 16; ++j)
					Serial.write(rawData[mouseAddress][j] == 0 ? 0 : 1);
				for (int j = 16; j < 27; ++j)
					Serial.write(rawData[controllerAddress][j] == 0 ? 0 : 1);
			}
			else if (currentReadPacket->commandAddress == joystickAddress)
			{
				for (int j = 0; j < 24; ++j)
					Serial.write(rawData[joystickAddress][j] == 0 ? 0 : 1);
			
				Serial.write("\n");    
			}
			else if (currentReadPacket->commandAddress == tabletAddress)
			{
				for (int j = 0; j < 40; ++j)
					Serial.write(rawData[tabletAddress][j] == 0 ? 0 : 1);			
				Serial.write("\n");    
			}		

			if (currentReadPacket->commandAddress == 0x02 || currentReadPacket->commandAddress == mouseAddress || currentReadPacket->commandAddress == controllerAddress)
			{
				for (int i = 0; i < 16; ++i)
				{
					Serial.write((rawData[0x02][i] & 0x0F) << 4);
					Serial.write((rawData[0x02][i] & 0xF0));
				}
				Serial.write("\n");        
			}
		}
#endif
#endif
		delete currentReadPacket;
	}
}

void adbStateChanged()
{
	diff = TCNT1 >> 1;

	if (state == WAITING_FOR_ATTENTION) {
		if (diff < 850 && diff > 750) {
			if (currentWritePacket == NULL)
				currentWritePacket = new Pippin_packet();
			state = WAITING_FOR_SYNC;
		}
	}
	else if (state == WAITING_FOR_SYNC) {
		//if (diff < 75 && diff > 55) { // The Sync pulse in practice appears to be shorter than expected
		currentWritePacket->syncTiming = diff;
		state = READING_COMMAND_BITS;
		count = 0;
		command = 0;
		//} else {
		//  state = WAITING_FOR_ATTENTION;
		//}
	}
	else if (state == READING_COMMAND_BITS) {
		if (count % 2 == 0 && (count / 2) < 8) {
			if (diff < 50) {
				command |= (1 << (7 - (count / 2)));
			}
		}
		count++;
		if (count >= 16) {
			currentWritePacket->commandType = ((command >> 2) & 3);
			currentWritePacket->commandAddress = (command >> 4);
			currentWritePacket->commandRegister = (command & 3);
			state = WAITING_FOR_CMD_STOP;
		}
	}
	else if (state == WAITING_FOR_CMD_STOP)
	{
		if ((diff < 75 && diff > 55) || (diff < 300 && diff > 150))
		{
			currentWritePacket->commandStop = true;
			state = WAITING_FOR_DATA_START;
		}
		else
		{
			state = WAITING_FOR_ATTENTION;
		}
	}
	else if (state == WAITING_FOR_DATA_START)
	{
		currentWritePacket->HasData = true;
		state = WAITING_DATA_START_BIT;
	}
	else if (state == WAITING_DATA_START_BIT)
	{
		if (diff > 25 && diff < 45)
		{
			currentWritePacket->dataStart = true;
		}
		else
		{	
			state = WAITING_FOR_ATTENTION;
		}
		count = -1;
		currentWritePacket->data[currentWritePacket->numBytes] = 0;
		state = READING_DATA_BITS;
	}
	else if (state == READING_DATA_BITS || state == COULD_BE_DATA_STOP_BIT)
	{
		state = READING_DATA_BITS;

		if (count % 2 == 0 && (count / 2) < 8)
		{
			if (diff < 50) // Is a 1, so can't be stop bit
				{
					currentWritePacket->data[currentWritePacket->numBytes] |= (1 << (7 - (count / 2)));
					state = READING_DATA_BITS;
				}
			else if (count == 0) // Is a 0 and the first bit after a full byte
				{
					state = COULD_BE_DATA_STOP_BIT;
				}
		}

		count++;
		if (count >= 16) {
			currentWritePacket->numBytes++;
			currentWritePacket->count += count;
			count = 0;
			currentWritePacket->data[currentWritePacket->numBytes] = 0;
		}
	}
	TCNT1 = 0;
}

void PippinSpy::writeSerial() {}

void PippinSpy::debugSerial() {}

void PippinSpy::updateState() {}

void PippinSpy::setup(byte controllerAddress, byte mouseAddress)
{
	for (int i = 3; i < 11; ++i)
		pinMode(i, INPUT_PULLUP);
	
	for (int i = 0; i < 27; ++i)
	{
		rawData[controllerAddress][i] = 1;
		rawData[mouseAddress][i] = 1;
		rawData[0x02][i] = 0;
	}
	
	lastMouseReportID = controllerAddress;
	initialControllerAddress = this->controllerAddress = controllerAddress;
	initialMouseAddress = this->mouseAddress = mouseAddress;
	pinMode(ADB_PIN, INPUT_PULLUP);
	attachInterrupt(digitalPinToInterrupt(ADB_PIN), adbStateChanged, CHANGE);
	Timer1.initialize(10000);
	Timer1.stop();
	Timer1.restart();
	Timer1.detachInterrupt();
}

static char startupBuffer[100];
const char* PippinSpy::startupMsg()
{
	char itoaBuff[10];
	strcpy(startupBuffer, "Pippin (");
	strcat(startupBuffer, itoa(initialControllerAddress, itoaBuff, 16));
	strcat(startupBuffer, ",");
	strcat(startupBuffer, itoa(initialMouseAddress, itoaBuff, 16));
	strcat(startupBuffer, ")");
	return startupBuffer;
}

#else
void PippinSpy::loop() {}
void PippinSpy::setup(byte controllerAddress, byte mouseAddress) {}
void PippinSpy::writeSerial() {}
void PippinSpy::debugSerial() {}
void PippinSpy::updateState() {}

const char* PippinSpy::startupMsg()
{
	return "Pippin (0,0)";
}

#endif

