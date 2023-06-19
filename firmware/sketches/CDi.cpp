//
// CDi.cpp
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

#include "CDi.h"

#if !defined(TP_PINCHANGEINTERRUPT) && defined(TP_IRREMOTE) && (defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO))

#define USE_IRREMOTE_HPP_AS_PLAIN_INCLUDE
#include <IRremote.hpp>

static unsigned long wireless_timeout;
static unsigned long wireless_remote_timeout;
static byte wireless_rawData[4];
static byte wireless_yaxis;
static byte wireless_xaxis;

static byte max_right;
static byte max_left;
static byte max_up;
static byte max_down;

static byte wired_rawData[3];
static byte wired_yAxis;
static byte wired_xAxis;
static unsigned long wired_timeout;

void CDiSpy::setup() {
	
	vSerial.begin(1200); 
	Serial2.begin(1200);
	
	// Initialization of IR to Serial Adapter
	vSerial.write(205);
	vSerial.write(192);
	vSerial.write(128);
	vSerial.write(128);
	delay(18);
	vSerial.write(205);
	vSerial.write(192);
	delay(18);
	vSerial.write(205);
	vSerial.write(192);
	vSerial.write(128);
	vSerial.write(128);
	
	max_right = max_left = max_up = max_down = 0;
  
	hasOutput = false;
	
	IrReceiver.begin(15);
	
#ifdef WIRELESS_DEBUG 
	  Serial.println(F("Ready to receive IR signals"));
#endif
	wireless_timeout = wired_timeout = wireless_remote_timeout = millis();
}

void CDiSpy::loop() {
	HandleSerial();
	HandleIR();
#ifndef WIRED_DEBUG
#ifndef WIRELESS_DEBUG
	printRawData();
#endif
#endif
}
void CDiSpy::writeSerial() {}
void CDiSpy::debugSerial() {}
void CDiSpy::updateState() {}

void CDiSpy::loop1()
{
	if (hasOutput)
	{				
		if (bits == 29)
		{
			value = value >> 1;
		}
		
		if ((protocolNum == RC6 && bits == 20) || 
			(protocolNum == RC5_CDI && bits == 29 && (value & 0b11111111111100000000000000000000) == 0b00001001001000000000000000000000))
		{
#ifdef WIRELESS_DEBUG
			for (int32_t i = 31; i >= 0; --i)
			{
				Serial.print((value & ((int32_t)1 << i)) != 0 ? "1" : "0");
				if (i % 8 == 0)
					Serial.print("|");
			}
			Serial.print("\n");
#else
			char button_pushed = 0;
			for (int i = 0; i < 8; ++i)
			{
				button_pushed |= (value & (1 << i));
			}
			
			byte outputCode = 0;
			
			switch (button_pushed)
			{
			case 0x03: // Pause
			case 0x30:
				wireless_rawData[2] |= 0b00001000;
				outputCode = 34;
				break;
			case 0x0C: // Standby
				wireless_rawData[2] |= 0b00010000;
				outputCode = 17;
				break;
			case 0x02: // Stop
			case 0x31:
				wireless_rawData[2] |= 0b00100000;
				outputCode = 33;
				break;
			case 0x014: // Previous Track
			case 0x21:
				wireless_rawData[2] |= 0b01000000;
				outputCode = 36;
				break;
			case 0x04: // Play
			case 0x2C:
				wireless_rawData[2] |= 0b10000000;
				outputCode = 32;
				break;
			case 0x20: // Next Track
				wireless_rawData[3] |= 0b00000001;
				outputCode = 35;
				break;
			case 0x10: // Next Track / Volume Up
				if ((value & 0b00001111111100000000) == 0b00000001100100000000)
				{
					wireless_rawData[3] |= 0b00010000;
					outputCode = 30;
				}
				else
				{
					wireless_rawData[3] |= 0b00000001;
					outputCode = 35;
				}
				break;
			case 0x17: // TV/CDI
			case 0x43: 
				wireless_rawData[3] |= 0b00000100;
				outputCode = 29;
				break;
			case 0x18: // Volume Down
			case 0x11:
				wireless_rawData[3] |= 0b00001000;
				outputCode = 31;
				break;
			case 0x19: // Volume Up
				wireless_rawData[3] |= 0b00010000;
				outputCode = 30;
				break;
			}
			wireless_remote_timeout = millis();
			
			//if (outputCode != 0 && (PINC & 0b00010000) != 0)
			if (outputCode != 0 && (flags & IRDATA_FLAGS_IS_REPEAT) == 0)
			{
				byte output[8];
				output[0] = 0xC0;
				output[1] = 0x80;
				output[2] = 0x80;
				output[3] = outputCode;
				output[4] = 0xC0;
				output[5] = 0x80;
				output[6] = 0x80;
				output[7] = 0;
				vSerial.write(output, 8);
			}
			hasOutput = false;
#endif      
		}
		else if((protocolNum == RC6 && bits == 36) ||
				 (protocolNum == RC5_CDI && bits == 29 && (value & 0b11111111111100000000000000000000) != 0b00001001001000000000000000000000))
		{
#ifdef WIRELESS_DEBUG
			for (int32_t i = 31; i >= 0; --i)
			{
				Serial.print((value & ((int32_t)1 << i)) != 0 ? "1" : "0");
				if (i % 8 == 0)
					Serial.print("|");
			}
			Serial.println();
#else
			wireless_yaxis = 0;
			for (int i = 0; i < 8; ++i)
			{
				wireless_yaxis |= (value & (1 << i));
			}
          
			if (wireless_yaxis == 0 || wireless_yaxis == 128)
			{
				wireless_rawData[0] = 0;
			}
			else if (wireless_yaxis > 128)
			{
				if (bits == 29)
					wireless_yaxis = 128 + 256 - wireless_yaxis;
				
				if (wireless_yaxis > max_up)
					max_up = wireless_yaxis;
          
				wireless_rawData[0] =  ScaleInteger(wireless_yaxis, 128, max_up, 0, 255);
				wireless_rawData[3] &= ~0b00100000;
			}
			else
			{
				if (wireless_yaxis > max_down)
					max_down = wireless_yaxis;
          
				wireless_rawData[0] =  ScaleInteger(wireless_yaxis, 0, max_down, 0, 255);
				wireless_rawData[3] |= 0b00100000;
			}
        
			wireless_xaxis = 0;
			for (int i = 8; i < 16; ++i)
			{
				wireless_xaxis |= ((value & (1 << i)) >> 8);
			} 
        
			if (wireless_xaxis == 0 || wireless_xaxis == 128)
			{
				wireless_rawData[1] = 0;
			}
			else if (wireless_xaxis > 128)
			{
				if (bits == 29)
					wireless_xaxis = 128 + 256 - wireless_xaxis;

				if (wireless_xaxis > max_left)
					max_left = wireless_xaxis;
            
				wireless_rawData[1] = ScaleInteger(wireless_xaxis, 128, max_left, 0, 255);
				wireless_rawData[3] &= ~0b01000000;
			}
			else
			{
				if (wireless_xaxis > max_right)
					max_right = wireless_xaxis;
            
				wireless_rawData[1] = ScaleInteger(wireless_xaxis, 0, max_right, 0, 255);
				wireless_rawData[3] |= 0b01000000;
			}

			bool button1 = (value & 0b00000000000000100000000000000000) != 0;
			bool button2 = (value & 0b00000000000000010000000000000000) != 0;
			wireless_rawData[2] = button1 ? (wireless_rawData[2] | 0x04) : (wireless_rawData[2] & ~0x04);
			wireless_rawData[2] = button2 ? (wireless_rawData[2] | 0x01) : (wireless_rawData[2] & ~0x01);
                      
			wireless_timeout = millis();
			
			//if ((~PINC & 0b00010000) != 0)
			if (true)
			{
				if (wireless_xaxis > 128)
					wireless_xaxis = 128 + 256 - wireless_xaxis;
				if ((wireless_xaxis & 0b00111111) == 0 && (wireless_xaxis & 0b10000000) != 0)
					wireless_xaxis = 255;
				
				if (wireless_yaxis > 128)
					wireless_yaxis = 128 + 256 - wireless_yaxis;
				if ((wireless_yaxis & 0b00111111) == 0 && (wireless_yaxis & 0b10000000) != 0)
					wireless_yaxis = 255;
				
				byte output[3];
				output[0] = 0xC0 | (button2 ? 0x20 : 0x0) 
								 | (button1 ? 0x10 : 0x0) 
								 | ((wireless_xaxis & 0b11000000) >> 6) 
								 | ((wireless_yaxis & 0b11000000) >> 4);
				output[1] = 0x80 | (wireless_xaxis & 0b00111111);
				output[2] = 0x80 | (wireless_yaxis & 0b00111111);
						
				//				Serial.print("o:");
				//				for (int i = 7; i >= 0; --i)
				//					Serial.print((output[0] & (1 << i)) ? "1" : "0");
				//				Serial.print("|");
				//				for (int i = 7; i >= 0; --i)
				//					Serial.print((output[1] & (1 << i)) ? "1" : "0");
				//				Serial.print("|");
				//				for (int i = 7; i >= 0; --i)
				//					Serial.print((output[2] & (1 << i)) ? "1" : "0");
				//				Serial.println();
				vSerial.write(output, 3);
			}
			hasOutput = false;
#endif
		}
	}
	
	if (((millis() - wireless_timeout) >= _wireless_timeout))
	{
		//if ((~PINC & 0b00010000) != 0 && (wireless_rawData[2] & 0b00000101) != 0)
		if (true && ((wireless_rawData[2] & 0b00000101) != 0))
		{
			byte output[3];
			output[0] = 0xC0;
			output[1] = 0x80;
			output[2] = 0x80;
			vSerial.write(output, 3);
		}
		
		for (int i = 0; i < 2; ++i)
			wireless_rawData[i] = 0;
		wireless_rawData[2] &= 0b11111010;
		wireless_xaxis = wireless_yaxis = 0;

	}
  
	if (((millis() - wireless_remote_timeout) >= _wireless_remote_timeout))
	{	
		wireless_rawData[2] &= 0b00000101;
		wireless_rawData[3] &= 0b01100000;
	}
}

void CDiSpy::HandleSerial()
{

	if (Serial2.available() >= 3) {

		char c = Serial2.read();
		if ((c & 0b11000000) == 0b11000000)
		{
#ifndef WIRED_DEBUG     
  
			wired_rawData[2] = (c & 0b00100000) != 0 ? (wired_rawData[2] | 0x01) : (wired_rawData[2] & ~0x01);    // Button 1
			wired_rawData[2] = (c & 0b00010000) != 0 ? (wired_rawData[2] | 0x04) : (wired_rawData[2] & ~0x04);    // Button 2
			wired_yAxis = ((c & 0b00001100) << 4);
			wired_xAxis = ((c & 0b00000011) << 6);
  
			c = Serial2.read();
			wired_xAxis = wired_xAxis + (byte)(c & 0b00111111);
  
			c = Serial2.read();
			wired_yAxis = wired_yAxis + (byte)(c & 0b00111111);
			
			if (wired_yAxis == 0 || wired_yAxis == 128)
			{
				wired_rawData[0] = 0;
			}
			else if (wired_yAxis > 128)
			{
				if (256 - wired_yAxis + 128 > max_up)
					max_up = 256 - wired_yAxis + 128;
          
				wired_rawData[0] =  ScaleInteger(256 - wired_yAxis + 128, 128, max_up, 0, 255);
				wired_rawData[2] &= ~0b00001000;
			}
			else
			{
				if (wired_yAxis > max_down)
					max_down = wired_yAxis;
          
				wired_rawData[0] =  ScaleInteger(wired_yAxis, 0, max_down, 0, 255);
				wired_rawData[2] |= 0b00001000;
			}
  
			if (wired_xAxis == 0 || wired_xAxis == 128)
			{
				wired_rawData[1] = 0;
			}
			else if (wired_xAxis > 128)
			{
				if (256 - wired_xAxis + 128 > max_left)
					max_left = 256 - wired_xAxis + 128;
             
				wired_rawData[1] = ScaleInteger(256 - wired_xAxis + 128, 128, max_left, 0, 255);
				wired_rawData[2] &= ~0b00010000;
			}
			else
			{
				if (wired_xAxis > max_right)
					max_right = wired_xAxis;
             
				wired_rawData[1] = ScaleInteger(wired_xAxis, 0, max_right, 0, 255);
				wired_rawData[2] |= 0b00010000;
			}
  
			wired_timeout = millis();
#else     
			Serial.print("i:");
			for (int i = 7; i >= 0; --i)
				Serial.print((c & (1 << i)) ? "1" : "0");
			Serial.print("|");
       
			for (int i = 0; i < 2; ++i) 
			{
				char c = Serial2.read();
				for (int i = 7; i >= 0; --i)
					Serial.print((c & (1 << i)) ? "1" : "0");
				if (i == 1)
					Serial.println();
				else
					Serial.print("|");
			}
#endif
		}
	}
	else if (((millis() - wired_timeout) >= _wired_timeout))
	{
		for (int i = 0; i < 2; ++i)
			wired_rawData[i] = 0;
		wired_xAxis = wired_yAxis = 0;

	}
}

void CDiSpy::HandleIR()
{
	if (IrReceiver.decode()) 
	{	
		bits = IrReceiver.decodedIRData.numberOfBits;
		value = IrReceiver.decodedIRData.decodedRawData;
		protocolNum = IrReceiver.decodedIRData.protocol;
		flags = IrReceiver.decodedIRData.flags;

		hasOutput = true;
	}
	IrReceiver.resume();
}

void CDiSpy::printRawData()
{
#ifdef WIRED_PRETTY_PRINT
	for (int i = 0; i < 3; ++i)
	{
		Serial.print(wired_rawData[i]);
		Serial.print("|"); 
	}

	Serial.print(wired_yAxis > 128 ? 256 - wired_yAxis + 128 : wired_yAxis);
	Serial.print("|"); 
	Serial.print(wired_xAxis > 128 ? 256 - wired_xAxis + 128 : wired_xAxis);
	Serial.print("\n"); 
#elif defined WIRELESS_PRETTY_PRINT
	for (int i = 0; i < 15; ++i)
	{
		Serial.print(wireless_rawData[i]);
		Serial.print("|"); 
	}
	Serial.print(wireless_yaxis);
	Serial.print("|"); 
	Serial.print(wireless_xaxis);
	Serial.print("\n");
#else
	int j = 0;
	for (int i = 0; i < 2; ++i)
	{
		if (wired_rawData[i] == 10)
			wired_rawData[i] = 11;
		Serial.write(wired_rawData[i]);
	}
	Serial.write(wired_rawData[2]);
	for (int i = 0; i < 2; ++i)
	{
		if (wireless_rawData[i] == 10)
			wireless_rawData[i] = 11;
		
		Serial.write(wireless_rawData[i]);
	}
	Serial.write(wireless_rawData[2]);
	Serial.write(wireless_rawData[3]);
	Serial.write("\n");
#endif
}

static char startupBuffer[100];

const char* CDiSpy::startupMsg()
{
	char itoaBuff[10];
	strcpy(startupBuffer, "CDi (");
	strcat(startupBuffer, itoa(_wired_timeout, itoaBuff, 10));
	strcat(startupBuffer, ",");
	strcat(startupBuffer, itoa(_wireless_timeout, itoaBuff, 10));
	strcat(startupBuffer, ")");
	return startupBuffer;
}

#endif