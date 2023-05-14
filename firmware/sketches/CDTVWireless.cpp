//
// CDTVWireless.h
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

#include "CDTVWireless.h"

#if defined(TP_IRREMOTE) && !(defined(__arm__) && defined(CORE_TEENSY))
#include <IRremote.h>

#define WIRELESS_TIMEOUT 100

static int RECV_PIN = 2;

static IRrecv irrecv(RECV_PIN);
static unsigned long long rawData;
static unsigned long wireless_timeout;

unsigned long GetBitMask_RemoteKeys(unsigned long received_code)
{
	byte bit = 0;
	switch (received_code)
	{
		// 1
	case 0x1ffe:
		bit = 6;
		break;
		// 2
	case 0x21fde:
		bit = 7;
		break;
		// 3
	case 0x11fee:
		bit = 8;
		break;
		// 4
	case 0x9ff6:
		bit = 9;
		break;
		// 5
	case 0x29fd6:
		bit = 10;
		break;
		// 6
	case 0x19fe6:
		bit = 11;
		break;
		// 7
	case 0x5ffa:
		bit = 12;
		break;
		// 8
	case 0x25fda:
		bit = 13;
		break;
		// 9
	case 0x15fea:
		bit = 14;
		break;
		// 0
	case 0x39fc6:
		bit = 15;
		break;

		// Escape
	case 0x31fce:
		bit = 16;
		break;
		// Enter
	case 0x35fca:
		bit = 17;
		break;

		// Genlock
	case 0x22fdd:
		bit = 18;
		break;
		// CD/TV
	case 0x2ffd:
		bit = 19;
		break;
		// Power
	case 0x12fed:
		bit = 20;
		break;

		// Rew
	case 0x32fcd:
		bit = 21;
		break;
		// Play/Pause
	case 0xaff5:
		bit = 22;
		break;
		// FF
	case 0x1afe5:
		bit = 23;
		break;
		// Stop
	case 0x2afd5:
		bit = 24;
		break;

		// Volume Up
	case 0x6ff9:
		bit = 25;
		break;
		// Volume Down
	case 0x3afc5:
		bit = 26;
		break;
	default:
		return 0;
		break;
	}
	return ((unsigned long long)1 << bit);

}

unsigned long long GetBitMask_ControllerKeys(unsigned long received_code)
{
	unsigned long long retVal = 0;

	int offset = ((received_code & 0b00000000000000000000100000000000) == 0) ? 27 : 0;

	if ((received_code & 0b00000000000000000000000000000100) == 0)
		retVal |= ((unsigned long long)1 << (2 + offset));

	if ((received_code & 0b00000000000000000000000000001000) == 0)
		retVal |= ((unsigned long long)1 << (0 + offset));

	if ((received_code & 0b00000000000000000000000000010000) == 0)
		retVal |= ((unsigned long long)1 << (3 + offset));

	if ((received_code & 0b00000000000000000000000000100000) == 0)
		retVal |= ((unsigned long long)1 << (1 + offset));

	if ((received_code & 0b00000000000000000000000001000000) == 0)
		retVal |= ((unsigned long long)1 << (4 + offset));

	if ((received_code & 0b00000000000000000000000010000000) == 0)
		retVal |= ((unsigned long long)1 << (5 + offset));

	return retVal;

}

void CDTVWirelessSpy::setup()
{
	rawData = 0;
	irrecv.enableIRIn();
}

void CDTVWirelessSpy::loop()
{
	decode_results results;
	if (irrecv.decode(&results))
	{
		if (results.decode_type == CDTV && results.bits == 24)
		{
			// Handle "remote control" keys
			if ((results.value & 0b0000000000000000000000000000011) != 0x03)
				rawData = GetBitMask_RemoteKeys(results.value);
			else
				rawData = GetBitMask_ControllerKeys(results.value);
			wireless_timeout = millis();
		}
		else if ((results.decode_type == NEC && results.bits == 0 && results.value == 0xFFFFFFFF)
			|| (results.decode_type == CDTV && results.bits == 4 && results.value == 0xFFFFFF))
		{
			wireless_timeout = millis();
		}
		irrecv.resume();
	}
	else if (((millis() - wireless_timeout) >= WIRELESS_TIMEOUT))
	{
		rawData = 0;
	}

#if !defined(DEBUG) || DEBUG == 0
	int checksum = 0;
	for (int i = 0; i < 33; ++i)
	{
		checksum += (rawData & ((unsigned long long)1 << i)) != 0 ? 1 : 0;
		Serial.write((rawData & ((unsigned long long)1 << i)) != 0 ? 1 : 0);
	}
	Serial.write((checksum & 0x0F) << 4);
	Serial.write((checksum & 0xF0));
	Serial.write("\n");
#else
	for (int i = 0; i < 33; ++i)
		Serial.print((rawData & ((unsigned long long)1 << i)) != 0 ? "1" : "0");
	Serial.print("\n");
#endif
}

void CDTVWirelessSpy::writeSerial() {}
void CDTVWirelessSpy::debugSerial() {}
void CDTVWirelessSpy::updateState() {}

#else

void CDTVWirelessSpy::setup() {}
void CDTVWirelessSpy::loop() {}
void CDTVWirelessSpy::writeSerial() {}
void CDTVWirelessSpy::debugSerial() {}
void CDTVWirelessSpy::updateState() {}

#endif

const char* CDTVWirelessSpy::startupMsg()
{
	return "CDTV Wireless";
}