//
// Nuon.cpp
//
// Author:
//       Christopher "Zoggins" Mallery <zoggins@retro-spy.com>
//
// Copyright (c) 2021 RetroSpy Technologies
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

//Enable for VERY verbose debugging output
//#define TRACE

#include "Nuon.h"
 
#if (defined(__arm__) && defined(CORE_TEENSY) && (defined(ARDUINO_TEENSY40) || defined(ARDUINO_TEENSY41))) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)

#include "elapsedMillis.h"

static signed char tmp;
static bool waitTmp = false;
static unsigned long lastReadTime;
static byte buffer[19];

FASTRUN void NuonSpy::writeSerial()
{	
	buffer[18] = '\n';
	
	if (max(micros() - lastReadTime, 0U) < 2000)
	{
		// Not enough time has elapsed, return
		return;
	}
	Serial.write(buffer, 19);
	lastReadTime = micros();
}

FASTRUN void NuonSpy::debugSerial()
{	
	buffer[18] = '\n';
	
	for (int i = 0; i < 64; ++i)
	{
		if (i % 8 == 0)
			Serial.print("|"); 
 
		Serial.print((rawData[i] == LOW) ? "0" : "1");
	}
	Serial.print("|");
	Serial.print((signed char)buffer[16]);
	Serial.print("|");
	Serial.print((signed char)buffer[17]);
	Serial.print("|");
	Serial.print(waitTmp);
	Serial.print("|");
	Serial.print(tmp);
	Serial.println(); 
}

FASTRUN void NuonSpy::updateState()
{
  
	elapsedMicros restPeriod = 0;

start:

	while (digitalReadFast(NUON_CLOCK_PIN) == HIGH) ;
	while (digitalReadFast(NUON_CLOCK_PIN) == LOW) ;

	if ((rawData[0] = digitalReadFast(NUON_DATA_PIN)) == LOW)
	{ 
		//Serial.println("here3");;
		goto start;
	}
	else if (restPeriod < 40)
	{
		//Serial.println("here4");
		restPeriod = 0;
		goto start;
	}
	else
	{
		restPeriod = 0;
   
		for (int i = 1; i < 64; ++i)
		{
			while (digitalReadFast(NUON_CLOCK_PIN) == HIGH) ;
			while (digitalReadFast(NUON_CLOCK_PIN) == LOW) ;
			rawData[i] = digitalReadFast(NUON_DATA_PIN);
		}
	}

	if ((rawData[0] == HIGH && rawData[1] == HIGH && rawData[2] == LOW && rawData[3] == LOW && rawData[4] == LOW
	      && rawData[5] == LOW && rawData[6] == HIGH && rawData[7] == LOW     
	      && rawData[11] == HIGH && rawData[12] == HIGH))
	{
      
		if (rawData[14] == LOW)
		{
			memcpy(buffer, &rawData[39], 16);
#if defined(DEBUG) || defined(TRACE)
			Serial.print("!");
			debugSerial();
			waitTmp = false;
#endif  
		}
		else
		{
			tmp = 0;
			for (int i = 0; i < 7; ++i)
				tmp |= (rawData[40 + i] == HIGH) ? ((1 << (6 - i))) : 0;
			if (rawData[39] == LOW)
				tmp |= 128;
          
			waitTmp = true;
        
#if defined(TRACE)
			Serial.print("$");
			debugSerial();
#endif          
		}
	}
	else if (waitTmp)
	{
		if (rawData[7] == HIGH && rawData[6] == HIGH && rawData[22] == LOW && rawData[31] == LOW)
		{
			tmp *= -1;
			if (tmp == 10)
				tmp = 11;
			buffer[17] = tmp;
		}
		else if (rawData[22] == LOW)
		{
			if (tmp == 10)
				tmp = 11;
			buffer[16] = tmp;
		}
		waitTmp = false;
#if defined(TRACE)
		Serial.print("#");
		debugSerial();
#endif
	}


}

FASTRUN void NuonSpy::loop()
{
	while (true)
	{
		noInterrupts();
		updateState();
		interrupts();
		
#if !defined(DEBUG) && !defined(TRACE)
		writeSerial();
#endif
	}
}

#else
void NuonSpy::loop()
{
	
}

void NuonSpy::writeSerial()
{
	
}

void NuonSpy::debugSerial()
{
	
}

void NuonSpy::updateState()
{
	
}
#endif

const char* NuonSpy::startupMsg()
{
	return "Nuon";
}