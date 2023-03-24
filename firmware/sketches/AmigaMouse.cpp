//
// AmgiaMouse.cpp
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

#include "AmigaMouse.h"

#if !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(ARDUINO_AVR_NANO_EVERY) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)

static byte buttons[3];
static int8_t currentX;
static int8_t currentY;
static int8_t lastX;
static int8_t lastY;
static byte lastEncoderX;
static byte lastEncoderY;


void AmigaMouseSpy::setup(byte videoOutputType, uint8_t cableType)
{
	currentX = lastX = 0;
	currentY = lastY = 0;
	
	this->cableType = cableType;

	// TIMER 1 for interrupt frequency 50 or 60 Hz:
	cli(); // stop interrupts
	TCCR1A = 0; // set entire TCCR1A register to 0
	TCCR1B = 0; // same for TCCR1B
	TCNT1 = 0; // initialize counter value to 0
	if (videoOutputType == VIDEO_NTSC)
	{  
		// set compare match register for 60.00060000600006 Hz increments
		OCR1A = 33332; // = 16000000 / (8 * 60.00060000600006) - 1 (must be <65536)
	}
	else // PAL
	{
		// set compare match register for 50 Hz increments
		OCR1A = 39999; // = 16000000 / (8 * 50) - 1 (must be <65536)
	}
	// turn on CTC mode
	TCCR1B |= (1 << WGM12);
	// Set CS12, CS11 and CS10 bits for 8 prescaler
	TCCR1B |= (0 << CS12) | (1 << CS11) | (0 << CS10);
	// enable timer compare interrupt
	TIMSK1 |= (1 << OCIE1A);
	sei(); // allow interrupts

}

#if !defined(COLECOVISION_ROLLER_TIMER_INT_HANDLER)
ISR(TIMER1_COMPA_vect) {
	int8_t x = lastX - currentX;
	int8_t y = lastY - currentY;

	lastX = currentX;
	lastY = currentY;

#ifdef DEBUG
	for (int i = 0; i < 3; ++i)
		Serial.print(buttons[i] == 0 ? "0" : "1");
	Serial.print('|');
	Serial.print(x);
	Serial.print('|');
	Serial.print(y);
	Serial.print('\n');
#else
	for (int i = 0; i < 3; ++i)
		Serial.write(buttons[i]);
	for (int i = 0; i < 8; ++i)
		Serial.write((x & (1 << i)) == 0 ? 0 : 1);
	for (int i = 0; i < 8; ++i)
		Serial.write((y & (1 << i)) == 0 ? 0 : 1);
	Serial.write('\n');
#endif
}
#endif

void AmigaMouseSpy::loop()
{
	noInterrupts();
	byte data = ~PIND;
	interrupts();

	byte xData = (data & 0b00101000);
	byte yData = (data & 0b00010100);

	if ((lastEncoderX == 0 && xData == 8)
		|| (lastEncoderX == 8 && xData == 40)
		|| (lastEncoderX == 40 && xData == 32)
		|| (lastEncoderX == 32 && xData == 0))
	{
		++currentX;
		lastEncoderX = xData;
	}
	else if ((lastEncoderX == 0 && xData == 32)
		|| (lastEncoderX == 32 && xData == 40)
		|| (lastEncoderX == 40 && xData == 8)
		|| (lastEncoderX == 8 && xData == 0))
	{
		--currentX;
		lastEncoderX = xData;
	}
	if ((lastEncoderY == 0 && yData == 4)
		|| (lastEncoderY == 4 && yData == 20)
		|| (lastEncoderY == 20 && yData == 16)
		|| (lastEncoderY == 16 && yData == 0))
	{
		++currentY;
		lastEncoderY = yData;

	}
	else if ((lastEncoderY == 0 && yData == 16)
		|| (lastEncoderY == 16 && yData == 20)
		|| (lastEncoderY == 20 && yData == 4)
		|| (lastEncoderY == 4 && yData == 0))
	{
		--currentY;
		lastEncoderY = yData;
	}

	if (cableType == CABLE_SMS)
	{
		buttons[0] = (data & 0b10000000) != 0 ? 1 : 0;
		buttons[1] = (data & 0b01000000) != 0 ? 1 : 0;
		buttons[2] = (PINB & 0b00000001) != 0 ? 0 : 1;		
	}
	else
	{
		buttons[0] = (data & 0b01000000) != 0 ? 1 : 0;
		buttons[1] = (PINB & 0b00000010) != 0 ? 1 : 0;
		buttons[2] = (data & 0b10000000) != 0 ? 1 : 0;	
	}

}

void AmigaMouseSpy::writeSerial() {}
void AmigaMouseSpy::debugSerial() {}
void AmigaMouseSpy::updateState() {}

const char* AmigaMouseSpy::startupMsg()
{
	return "Amiga Mouse";
}


#else
void AmigaMouseSpy::loop() {}
void AmigaMouseSpy::setup(byte videoOutputType, uint8_t cableType) {}
void AmigaMouseSpy::writeSerial() {}
void AmigaMouseSpy::debugSerial() {}
void AmigaMouseSpy::updateState() {}

const char* AmigaMouseSpy::startupMsg()
{
	return "Mode not compatible with this hardware";
}

#endif

