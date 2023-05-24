//
// CDi.h
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

#ifndef CDi_h
#define CDi_h

#include "ControllerSpy.h"

// Uncomment 1 of these for different levels of debugging output
// I would not recomend using more than 1 at a time!
//#define WIRED_PRETTY_PRINT
//#define WIRELESS_PRETTY_PRINT
//#define WIRED_DEBUG
//#define WIRELESS_DEBUG

#if !defined(TP_PINCHANGEINTERRUPT) && defined(TP_IRLIB2) && !(defined(__arm__) && defined(CORE_TEENSY))

#include <SoftwareSerial.h>
#include <IRLibDecodeBase.h> 
#include <IRLib_P04_RC6.h>   
#include <IRLib_P13_RC5_CDi.h>  
#include <IRLib_P14_CDTV.h>
#include <IRLibCombo.h>     // After all protocols, include this
#include <IRLibRecvPCI.h>

class CDiSpy : public ControllerSpy {
public:
	CDiSpy(int wired_timeout, int wireless_timeout)
		: myReceiver(CDI_IRPIN)
		, vSerial(CDI_RECVSER, CDI_SENDSER, true)
		, _wired_timeout(wired_timeout)
		, _wireless_timeout(wireless_timeout)
	{}

	void setup();
	void loop();
	void writeSerial();
	void debugSerial();
	void updateState();
	
	virtual const char* startupMsg();

private:
	
	void HandleSerial();
	void HandleIR();
	void printRawData();
	
	int _wired_timeout;
	int _wireless_timeout;
	IRrecvPCI myReceiver;
	SoftwareSerial vSerial;
	IRdecode myDecoder; 
};
#else

class CDiSpy : public ControllerSpy {
public:
	CDiSpy(int wired_timeout, int wireless_timeout) {}

	void setup(){}
	
	void loop() {}
	void writeSerial() {}
	void debugSerial() {}
	void updateState() {}
	
	virtual const char* startupMsg()
	{
		return "CDi Firmware Not Supported";
	}

private:
	
};

#endif
#endif