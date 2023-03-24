//
// Wii.h
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

#ifndef WiiSpy_h
#define WiiSpy_h

#include "ControllerSpy.h"

#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
typedef uint32_t port_t;
#else
typedef uint8_t port_t;
#endif

class WiiSpy : public ControllerSpy {
public:
	void setup();
	void setup1();
	void loop();
	void loop1();
	void writeSerial();
	void debugSerial();
	void updateState();
	const char* startupMsg();

private:
	port_t    current_port = 0;
	port_t    last_port;
	int       i2c_index = 0;
	bool      isControllerPoll = false;
	bool      isControllerID = false;
	bool      isEncrypted = false;
	byte      encryptionKeySet = 0;
	bool      isKeyThing = false;
	byte      keyThing[8];
	byte      cleanData[274];
	byte      rawData[16000];
	byte      sendData[51];
	volatile bool sendRequest = false;
};

#endif
