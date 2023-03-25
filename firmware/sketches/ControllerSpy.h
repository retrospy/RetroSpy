//
// ControllerSpy.h
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

#ifndef ControllerSpy_h
#define ControllerSpy_h

#include "common.h"
#include <string.h>

class ControllerSpy {
public:
	virtual void setup()
	{
		common_pin_setup();
	}
	
	virtual void printFirmwareInfo()
	{
		delay(1000);
		Serial.print("// Starting up in ");
		Serial.print(startupMsg());
		Serial.println(" mode");
		Serial.print("// Version: ");
		Serial.println("6.1.4"); /*VERSIONINFO*/
		delay(1000);		
	}
	
	virtual void setup1() {}
	virtual void loop() = 0;
	virtual void loop1() {}
	virtual void writeSerial() = 0;
	virtual void debugSerial() = 0;
	virtual void updateState() = 0;
	virtual const char* startupMsg() { return "Default Startup Message";}; 
};

#endif
