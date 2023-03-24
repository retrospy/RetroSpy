//
// CDiKeyboard.cpp
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

#include "CDiKeyboard.h"

#if !defined(TP_PINCHANGEINTERRUPT) && !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)

// Backspace == Delete
static byte lookup[128] = {
  67,68,69,70,71,72,74,75,
  63,47,64,59,66,54,-1,-1,
  -1,-1,-1,-1,-1,-1,-1,-1,
  -1,47,-1,0,-1,-1,16,-1,

  61,2,53,4,5,6,8,53,
  11,12,10,14,55,13,56,58,
  12,2,3,4,5,6,7,8,
  10,11,52,52,55,14,56,58,

  3,18,19,20,21,22,23,24,
  26,27,28,29,30,31,32,34,
  35,36,37,38,39,40,42,43,
  44,45,46,48,51,50,7,13,

  60,18,19,20,21,22,23,24,
  26,27,28,29,30,31,32,34,
  35,36,37,38,39,40,42,43,
  44,45,46,48,51,50,60,15
};

void CDiKeyboardSpy::setup() {
	vSerial.begin(1200);
}

void CDiKeyboardSpy::loop() {
	updateState();
#if !defined(DEBUG)
	writeSerial();
#endif
}

void CDiKeyboardSpy::writeSerial() 
{
	for (int i = 0; i < 10; ++i)
		Serial.write(rawData[i]);
	Serial.write('\n');
}

void CDiKeyboardSpy::debugSerial() {
	
}

void CDiKeyboardSpy::updateState() {

	if (vSerial.available() >= 2) 
	{
		// clear the pressed keys
		for(int i = 0 ; i < 10 ; ++i)
		  rawData[i] = 0;
      
		incomingBytes[0] = vSerial.read();
		incomingBytes[1] = vSerial.read();

		if ((incomingBytes[0] & 0b11000000) == 0b11000000 && (incomingBytes[1] & 0b11011111) > 0x5F)
		{
			while (vSerial.available() < 2) {}
			incomingBytes[2] = vSerial.read();
			incomingBytes[3] = vSerial.read();
  
			bool isModifiedFunctionKey = false, blank_f1 = false, caps = false, sshift = false, shift = false, ctrl = false;
      
			if ((0b00100000 & incomingBytes[2]) != 0)
			{
				rawData[76 / 8] |= (1 << (76 % 8));
				caps = T_mode_caps_on = true;
			}
			else if (T_mode_caps_on)
			{
				blank_f1 = true;
				T_mode_caps_on = false;
			}
      
			if ((0b00010000 & incomingBytes[2]) != 0)
			{
				if (incomingBytes[3] - 128 >= 8 && incomingBytes[3] - 128 < 16 && (incomingBytes[2] & 0b00000010) != 0)
				{
					isModifiedFunctionKey = true;
					incomingBytes[3] -= 8;
				}
        
				rawData[77 / 8] |= (1 << (77 % 8));
				shift = true;
			}
      
			if ((0b00000001 & incomingBytes[1]) != 0)
			{      
				if (incomingBytes[3] - 128 >= 16 && incomingBytes[3] - 128 < 24 && (incomingBytes[2] & 0b00000010) != 0)
				{
					isModifiedFunctionKey = true;
					incomingBytes[3] -= 16;
				}
        
				rawData[78 / 8] |= (1 << (78 % 8));
				sshift = true;
			}
  
			if ((0b00000010 & incomingBytes[1]) != 0)
			{ 
 
				if ((incomingBytes[3] - 128 == 0 || incomingBytes[3] == 158 
				      || incomingBytes[3] == 130 || incomingBytes[3] == 131 
				      || incomingBytes[3] == 132 || incomingBytes[3] == 133) 
					&& (incomingBytes[2] & 0b00000010) == 0)
					incomingBytes[2] |= 0b00000100;
				else if (incomingBytes[3] - 128 >= 1 && incomingBytes[3] - 128 <= 26 && (incomingBytes[2] & 0b00000010) == 0)
					incomingBytes[3] += 64; 
          
				else if (incomingBytes[3] - 128 >= 24 && incomingBytes[3] - 128 < 32 && (incomingBytes[2] & 0b00000010) != 0)
				{
					incomingBytes[3] -= 24;
					isModifiedFunctionKey = true;
				}
          
				rawData[79 / 8] |= (1 << (79 % 8));
				ctrl = true;
			}   
  
			if ((0b00000100 & incomingBytes[2]) == 0)
			{
				if ((0b00000001 & incomingBytes[2]) != 0)
				{
					rawData[lookup[(incomingBytes[3] - 64)] / 8] |= (1 << lookup[incomingBytes[3] - 64] % 8);
				}
				else if (isModifiedFunctionKey || !((caps || shift || sshift || ctrl) && incomingBytes[3] == 128))
				{
					rawData[lookup[(incomingBytes[3] - 128)] / 8] |= (1 << lookup[incomingBytes[3] - 128] % 8);
				}
			}

			if (blank_f1)
				rawData[67 / 8] &= ~(1 << (67 % 8));

			//    for(int i = 0; i < 4; ++i)
			//    {
			//      Serial.print(incomingBytes[i]);
			//      Serial.print(" ");
			//      for(int j = 0; j < 8; ++j)
			//        Serial.print((incomingBytes[i] & (1 << j)) != 0 ? "1" : "0");
			//      Serial.print("\n");
			//    }
			//    Serial.print("\n");
      
		}
		else if ((incomingBytes[0] & 0b10000000) == 0b10000000) // K-Mode
		{
			if ((0b00010000 & incomingBytes[0]) != 0)  //Caps
			{
				rawData[76 / 8] |= (1 << (76 % 8));
			}
      
			if ((0b00001000 & incomingBytes[0]) != 0)  // Shift
			{
				if (incomingBytes[1] >= 8 && incomingBytes[1] < 16 && (incomingBytes[0] & 0b00000001) != 0)
					incomingBytes[1] -= 8;
          
				rawData[77 / 8] |= (1 << (77 % 8));
			}
      
			if ((0b00100000 & incomingBytes[0]) != 0)  // Super Shift
			{
				if (incomingBytes[1] >= 16 && incomingBytes[1] < 24 && (incomingBytes[0] & 0b00000001) != 0)
					incomingBytes[1] -= 16;
				rawData[78 / 8] |= (1 << (78 % 8));
			}
  
			if ((0b01000000 & incomingBytes[0]) != 0)  // Ctrl
			{
				if ((incomingBytes[1] == 0 || incomingBytes[1] == 158 - 128 
						|| incomingBytes[1] == 130 - 128 || incomingBytes[1] == 131 - 128 
						|| incomingBytes[1] == 132 - 128 || incomingBytes[1] == 133 - 128)  
					&& (incomingBytes[0] & 0b00000001) == 0)
					incomingBytes[0] |= 0b00000010;
          
				else if (incomingBytes[1] >= 1 && incomingBytes[1] <= 26 && (incomingBytes[0] & 0b00000001) == 0)
					incomingBytes[1] += 64; 
          
				else if (incomingBytes[1] >= 24 && incomingBytes[1] < 32 && (incomingBytes[0] & 0b00000001) != 0)
					incomingBytes[1] -= 24;
          
				rawData[79 / 8] |= (1 << (79 % 8));
			}
      
			if ((0b00000010 & incomingBytes[0]) == 0)
			{     
				if (lookup[incomingBytes[1]] != -1)
				{
					rawData[lookup[incomingBytes[1]] / 8] |= (1 << lookup[incomingBytes[1]] % 8);
				}
			}

			//    for(int i = 0; i < 2; ++i)
			//    {
			//      Serial.print(incomingBytes[i]);
			//      Serial.print(" ");
			//      for(int j = 0; j < 8; ++j)
			//        Serial.print((incomingBytes[i] & (1 << j)) != 0 ? "1" : "0");
			//      Serial.print("\n");
			//    }
			//    Serial.print("\n");
		}
	}
}

#endif

const char* CDiKeyboardSpy::startupMsg()
{
	return "CDi Keyboard";
}