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

#ifndef GCSpy_h
#define GCSpy_h

#include "ControllerSpy.h"

class GCSpy : public ControllerSpy {
public:
	void loop();
	void loop1();
	void writeSerial();
	void debugSerial();
	void updateState();
	
	virtual const char* startupMsg();

private:
	bool seenGC2N64 = false;
	bool checkPrefixGBA();
	bool checkPrefixGC();
	bool checkPrefixKeyboard();
	bool checkBothGCPrefixOnRaphnet();
	void sendRawGBAData();
	bool checkPrefixLuigisMansion();
	bool checkPrefixSNES2GC();
	
	void debugKeyboard();
	void writeKeyboard();
	
	unsigned char rawData[34 + GC_PREFIX + GC_BITCOUNT];
	unsigned char readBits;
	
	unsigned char sendData[34 + GC_PREFIX + GC_BITCOUNT];
	volatile bool sendRequest = false;
	short sendHeaderVal = 0;
};

#endif
