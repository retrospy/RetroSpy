//
// SMS.h
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

#ifndef SMSSpy_h
#define SMSSpy_h

#include "ControllerSpy.h"

class SMSSpy : public ControllerSpy {
public:
	void setup(uint8_t cableType, uint8_t outputType = OUTPUT_SMS);
	void setup();
	void loop();
	void writeSerial();
	void debugSerial();
	void updateState();
	
	virtual const char* startupMsg();

	enum cableTypes {
		CABLE_SMS = 1,
		CABLE_GENESIS = 2,
		CABLE_GX4000 = 3,
		CABLE_7800 = 4
	};

	enum outputTypes {
		OUTPUT_SMS     = 1,
		OUTPUT_GENESIS = 2
	};
	
private:
	enum buttonTypes {
		CC_BTN_UP = 1,
		CC_BTN_DOWN = 2,
		CC_BTN_LEFT = 4,
		CC_BTN_RIGHT = 8,
		CC_BTN_1 = 16,
		CC_BTN_2 = 32,
		CC_BTN_3 = 64
	};

	uint8_t cableType = CABLE_SMS;
	uint8_t outputType = OUTPUT_SMS;

	bool convertOutputToGenesis = false;
	
	static const byte CC_INPUT_PINS = 7;
	static const unsigned long CC_READ_DELAY_MS = 5;

	byte inputPins[CC_INPUT_PINS];
	word currentState;
	word lastState = -1;
	unsigned long lastReadTime;
};

#endif
