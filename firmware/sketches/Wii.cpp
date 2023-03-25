//
// Wii.cpp
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

#include "Wii.h"

#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
#define PIN_SCL		13
#define PIN_SDA		12
#define BIT_SCL		(1UL << PIN_SCL)
#define BIT_SDA		(1UL << PIN_SDA)
#else
#define PIN_SCL		WII_SCL
#define PIN_SDA		WII_SDA
#define BIT_SCL		(1 << WII_BIT_SCL)
#define BIT_SDA		(1 << WII_BIT_SDA)
#endif

#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
#define READ_PINS gpio_get_all()
#elif defined(__arm__) && defined(CORE_TEENSY) && defined(ARDUINO_TEENSY35)
#define READ_PINS GPIOB_PDIR
#elif defined(__arm__) && defined(CORE_TEENSY) && (defined(ARDUINO_TEENSY40) || defined(ARDUINO_TEENSY41))
#define READ_PINS GPIO6_PSR
#endif

#if defined(__arm__) && defined(CORE_TEENSY) && (defined(ARDUINO_TEENSY35) || defined(ARDUINO_TEENSY40) || defined(ARDUINO_TEENSY41)) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
void WiiSpy::setup() {
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	setup1();
#endif

	delay(1000);
	Serial.println(startupMsg());
	delay(1000);
}

void WiiSpy::setup1() {
	pinMode(PIN_SDA, INPUT);
	pinMode(PIN_SCL, INPUT);

	cleanData[0] = 2;
	cleanData[1] = -1;
	cleanData[46] = '\n';
	cleanData[50] = '\n';
}

void WiiSpy::loop() {
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	loop1();
#endif
	
	if (sendRequest)
	{
		memcpy(sendData, cleanData, 51);
		sendRequest = false;
#ifdef DEBUG
		debugSerial();
#else
		writeSerial();
#endif
	}
}

void WiiSpy::loop1()
{
	
	
	last_port = current_port;
	noInterrupts();
	current_port = READ_PINS & (BIT_SCL|BIT_SDA);
	interrupts();
	bool bDataReady = current_port != last_port;

	if (bDataReady)
	{

		if ((last_port == (BIT_SCL|BIT_SDA)) && (current_port == BIT_SCL))
		{
			// START
			i2c_index = 0;
		}
		else if ((last_port == BIT_SCL) && (current_port == (BIT_SCL|BIT_SDA)))
		{
			
			// STOP
			i2c_index -= (i2c_index % 9);

			byte tempData[128];
			tempData[0] = 0;
			for (int i = 0; i < 7; ++i)
			{
				if (rawData[i] != 0)
					tempData[0] |= 1 << (6 - i);
			}

			if (tempData[0] != 0x52) return;
			
			bool _isControllerID = isControllerID;
			bool _isControllerPoll = isControllerPoll;
			isControllerID = false;
			isControllerPoll = false;

			if (rawData[8] != 0) return;
			
			bool isWrite = rawData[7] == 0;

			int i = 9;
			byte numbytes = 1;
			while (i < i2c_index)
			{
				tempData[numbytes] = 0;
				for (int j = 0; j < 8; ++j)
				{
					if (rawData[j + i] != 0)
						tempData[numbytes] |= 1 << (7 - j);
				}
				++numbytes;

				if (!isWrite && i + 8 == i2c_index - 1)
				{
					if (rawData[i + 8] == 0) return;  // Last byte of read ends with NACK
				}
				else
				{
					if (rawData[i + 8] != 0) return; // Every other byte ends with ACK
				}
				i += 9;
			}

			while (sendRequest)
			{
			}
			if (isWrite)
			{
				if (numbytes == 2 && tempData[1] == 0)
				{
					isControllerPoll = true;
				}
				else if (numbytes == 3 && tempData[1] == 0xF0 && tempData[2] == 0x55)
				{
					isEncrypted = false;
					cleanData[1] = -1;
				}
				else if (numbytes == 2 && tempData[1] == 0xFA)
				{
					isControllerID = true;
				}
				else if (numbytes == 2 && (tempData[1] == 0x20 || tempData[1] == 0x30))
				{
					isKeyThing = true;
				}
				else if (numbytes == 8 && tempData[1] == 0x40)
				{
					int j = 2;
					for (int i = 0; i < 6; i++)
					{
						cleanData[j] = (tempData[2 + i] & 0xF0);
						cleanData[j + 1] = ((tempData[2 + i] & 0x0F) << 4);
						j += 2;
					}
				}
				else if (numbytes == 8 && tempData[1] == 0x46)
				{
					int j = 14;
					for (int i = 0; i < 6; i++)
					{
						cleanData[j] = (tempData[2 + i] & 0xF0);
						cleanData[j + 1] = ((tempData[2 + i] & 0x0F) << 4);
						j += 2;
					}
				}
				else if (numbytes == 6 && tempData[1] == 0x4C)
				{
					int j = 26;
					for (int i = 0; i < 4; i++)
					{
						cleanData[j] = (tempData[2 + i] & 0xF0);
						cleanData[j + 1] = ((tempData[2 + i] & 0x0F) << 4);
						j += 2;
					}
					isEncrypted = true;
					encryptionKeySet = (encryptionKeySet + 1) % 255;
					if (encryptionKeySet == 10)
						encryptionKeySet = 11;
					cleanData[1] = encryptionKeySet;
				}
			}
			else
			{
				// This is a hack, to handle a problem I don't fully understand
				if (isKeyThing && numbytes == 9)
				{
					keyThing[7] = tempData[1];
					for (int i = 0; i < 7; ++i)
						keyThing[i] = tempData[i + 2];
					isKeyThing = false;
				}
				else if (_isControllerID && (numbytes == 7 || numbytes == 9))
				{
					if (tempData[numbytes - 2] == 0 && tempData[numbytes - 1] == 0)
					{
						cleanData[0] = 0;
					}
					else if (tempData[numbytes - 2] == 1 && tempData[numbytes - 1] == 1)
					{
						cleanData[0] = 1;
					}
					else
						cleanData[0] = 2;
				}
				else if (_isControllerPoll && numbytes >= 7)
				{
					// This is a hack, to handle a problem I don't fully understand
					int  numZeroes = 0;
					int  numMatch = 0;
					if (numbytes == 9)
					{
						for (int i = 1; i < 9; ++i)
						{
							if (tempData[i] == keyThing[i - 1])
								++numMatch;
							if (tempData[i] == 0)
								++numZeroes;
						}
						if (numZeroes == 8 || numMatch == 8) return;
					}

					int j = 34;
					if (numbytes == 11)
					{
						cleanData[0] = 3;
						for (int i = 0; i < 8; i++)
						{
							cleanData[j] = (tempData[1 + i] & 0xF0);
							cleanData[j + 1] = (tempData[1 + i] << 4);
							j += 2;
						}
					}
					else
					{
						// NES/SNES Classic return 22 bytes and have the data offset by 2 bytes
						int offset = numbytes == 22 ? 3 : 1;
						for (int i = 0; i < 6; i++)
						{
							cleanData[j] = (tempData[offset + i] & 0xF0);
							cleanData[j + 1] = (tempData[offset + i] << 4);
							j += 2;
						}
					}
					sendRequest = true;
				}
			}
		}
		else if ((last_port == BIT_SDA) && (current_port == (BIT_SCL|BIT_SDA)))
		{
			// ONE
			rawData[i2c_index++] = 1;
		}
		else if ((last_port == 0x0) && (current_port == BIT_SCL))
		{
			// ZERO
			rawData[i2c_index++] = 0;
		}
	}
}

void WiiSpy::writeSerial()
{
	if (sendData[0] == 3)
		Serial.write(sendData, 51);
	else
		Serial.write(sendData, 47);
}

void WiiSpy::debugSerial()
{
	Serial.print(sendData[0]);
	Serial.print(' ');
	Serial.print(sendData[1]);
	Serial.print(' ');
	int j = 2;
	int toPrint = 22;
	if (sendData[0] == 3)
	{
		toPrint = 26;
	}
	for (int i = 0; i < toPrint; ++i)
	{
		byte data = (sendData[j] | (sendData[j + 1] >> 4));
		Serial.print(data);
		Serial.print(' ');
		j += 2;
	}
	Serial.print('\n');
}

void WiiSpy::updateState() {
}


#else
void WiiSpy::setup() {
}

void WiiSpy::loop() {
}

void WiiSpy::writeSerial() {
}

void WiiSpy::debugSerial() {
}

void WiiSpy::updateState() {
}

#endif

const char* WiiSpy::startupMsg()
{
	return "Wii";
}