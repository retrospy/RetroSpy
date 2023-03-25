//
// SNES.h
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

#ifndef SNESSpy_h
#define SNESSpy_h

// ---------------------------------------------------------------------------------
// The only reason you'd want to use 2-wire mode is if you built a NintendoSpy
// before the 3-wire firmware was implemented.  This mode is for backwards
// compatibility only.
//#define MODE_2WIRE_SNES
// ---------------------------------------------------------------------------------

#include "ControllerSpy.h"

class SNESSpy : public ControllerSpy {
public:
	void setup1();
	void loop();
	void loop1();
	void writeSerial();
	void debugSerial();
	void updateState();
	
	virtual const char* startupMsg();

private:
	unsigned char rawData[SNES_BITCOUNT_EXT];
	unsigned char bytesToReturn = SNES_BITCOUNT;
    unsigned char sendData[SNES_BITCOUNT_EXT];
    unsigned char sendBytes = SNES_BITCOUNT;
    volatile bool sendRequest = false;
};

#endif
