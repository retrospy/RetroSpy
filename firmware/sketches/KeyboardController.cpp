//
// KeyboardController.cpp
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

#include "KeyboardController.h"

#if (defined(TP_PINCHANGEINTERRUPT) && !(defined(__arm__) && defined(CORE_TEENSY))) || defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
#include <PinChangeInterrupt.h>
#include <PinChangeInterruptBoards.h>
#include <PinChangeInterruptPins.h>
#include <PinChangeInterruptSettings.h>
#endif

// The below values are not scientific, but they seem to work.  These may need to be tuned for different systems.
#define LINE_WAIT 200
#define DIGITAL_HIGH_THRESHOLD 150
#define PICO_DIGITAL_HIGH_THRESHOLD 500

static volatile byte currentState = 0;
static byte lastState = 0xFF;
static byte lastRawData = 0;

static byte rawData;

void row1_isr_vision()
{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	delayMicroseconds(LINE_WAIT);
#else
	for (int i = 0; i < 25*LINE_WAIT; ++i)  // This is trial and error'd.  
		asm volatile("nop\n");    // NOP isn't consistent enough on an optimized Pi Pico
#endif
	byte cachedCurrentState = currentState;
	if (currentState > 3)
		return;
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	else if (PIN_READ(6) == 0)
		currentState = 3;
	else if (PIN_READ(7) == 0)
		currentState = 2;
	else if (PINB_READ(1) == 0)
		currentState = 1;
#else
	else if (PIN_READ(4) == 0)
		currentState = 3;
	else if (PIN_READ(5) == 0)
		currentState = 2;
	else if (PINB_READ(7) == 0)
		currentState = 1;
#endif
	else if (cachedCurrentState >= 1 && cachedCurrentState <= 3)
		currentState = 0;
}

void row1_isr_legacy()
{
	delayMicroseconds(LINE_WAIT);
	byte cachedCurrentState = currentState;
	if (currentState > 3)
		return;
	else if (PIN_READ(7) == 0)
		currentState = 3;
	else if (PINB_READ(0) == 0)
		currentState = 2;
	else if (PIN_READ(6) == 0)
		currentState = 1;
	else if (cachedCurrentState >= 1 && cachedCurrentState <= 3)
		currentState = 0;
}

void row2_isr_legacy()
{
	delayMicroseconds(LINE_WAIT);
	byte cachedCurrentState = currentState;
	if (currentState > 6)
		return;
	else if (PIN_READ(7) == 0)
		currentState = 6;
	else if (PINB_READ(0) == 0)
		currentState = 5;
	else if (PIN_READ(6) == 0)
		currentState = 4;
	else if (cachedCurrentState >= 4 && cachedCurrentState <= 6)
		currentState = 0;
}

void row2_isr_vision()
{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	delayMicroseconds(LINE_WAIT);
#else
	for (int i = 0; i < 25*LINE_WAIT; ++i)  // This is trial and error'd.  
		asm volatile("nop\n");    // NOP isn't consistent enough on an optimized Pi Pico
#endif
	byte cachedCurrentState = currentState;
	if (currentState > 6)
		return;
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	else if (PIN_READ(6) == 0)
		currentState = 6;
	else if (PIN_READ(7) == 0)
		currentState = 5;
	else if (PINB_READ(1) == 0)
		currentState = 4;
#else
	else if (PIN_READ(4) == 0)
		currentState = 6;
	else if (PIN_READ(5) == 0)
		currentState = 5;
	else if (PINB_READ(7) == 0)
		currentState = 4;
#endif
	else if (cachedCurrentState >= 4 && cachedCurrentState <= 6)
		currentState = 0;
}

void row3_isr_legacy()
{
	delayMicroseconds(LINE_WAIT);
	byte cachedCurrentState = currentState;
	if (currentState > 9)
		return;
	else if (PIN_READ(7) == 0)
		currentState = 9;
	else if (PINB_READ(0) == 0)
		currentState = 8;
	else if (PIN_READ(6) == 0)
		currentState = 7;
	else if (cachedCurrentState >= 7 && cachedCurrentState <= 9)
		currentState = 0;
}

void row3_isr_vision()
{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	delayMicroseconds(LINE_WAIT);
#else
	for (int i = 0; i < 25*LINE_WAIT; ++i)  // This is trial and error'd.  
		asm volatile("nop\n");    // NOP isn't consistent enough on an optimized Pi Pico
#endif
	byte cachedCurrentState = currentState;
	if (currentState > 9)
		return;
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	else if (PIN_READ(6) == 0)
		currentState = 9;
	else if (PIN_READ(7) == 0)
		currentState = 8;
	else if (PINB_READ(1) == 0)
		currentState = 7;
#else
	else if (PIN_READ(4) == 0)
		currentState = 9;
	else if (PIN_READ(5) == 0)
		currentState = 8;
	else if (PINB_READ(7) == 0)
		currentState = 7;
#endif
	else if (cachedCurrentState >= 7 && cachedCurrentState <= 9)
		currentState = 0;
}

void row4_isr_legacy()
{
	delayMicroseconds(LINE_WAIT);
	byte cachedCurrentState = currentState;
	if (PIN_READ(7) == 0)
		currentState = 12;
	else if (PINB_READ(0) == 0)
		currentState = 11;
	else if (PIN_READ(6) == 0)
		currentState = 10;
	else if (cachedCurrentState >= 10)
		currentState = 0;
}

void row4_isr_vision()
{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	delayMicroseconds(LINE_WAIT);
#else
	for (int i = 0; i < 25*LINE_WAIT; ++i)  // This is trial and error'd.  
		asm volatile("nop\n");    // NOP isn't consistent enough on an optimized Pi Pico
#endif

	byte cachedCurrentState = currentState;
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	if (PIN_READ(6) == 0)
		currentState = 12;
	else if (PIN_READ(7) == 0)
		currentState = 11;
	else if (PINB_READ(1) == 0)
		currentState = 10;
#else
	if (PIN_READ(4) == 0)
		currentState = 12;
	else if (PIN_READ(5) == 0)
		currentState = 11;
	else if (PINB_READ(7) == 0)
		currentState = 10;
#endif
	else if (cachedCurrentState >= 10)
		currentState = 0;
}

void sr_row1sr_isr_legacy()
{
	delayMicroseconds(LINE_WAIT);
	byte cachedCurrentState = currentState;
	if (currentState > 3)
		return;
	else if (PIN_READ(7) == 0)
		currentState = 3;
	else if (analogRead(1) < DIGITAL_HIGH_THRESHOLD)
		currentState = 2;
	else if (analogRead(0) < DIGITAL_HIGH_THRESHOLD)
		currentState = 1;
	else if (cachedCurrentState >= 1 && cachedCurrentState <= 3)
		currentState = 0;
}

void sr_row1sr_isr_vision()
{

#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	delayMicroseconds(LINE_WAIT);
#else
	for (int i = 0; i < 25*LINE_WAIT; ++i)  // This is trial and error'd.  
		asm volatile("nop\n");    // NOP isn't consistent enough on an optimized Pi Pico
#endif
	
	byte cachedCurrentState = currentState;
	if (currentState > 3)
		return;
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	else if (PIN_READ(6) == 0)
		currentState = 3;
	else if (analogRead(7) < DIGITAL_HIGH_THRESHOLD)
		currentState = 2;
	else if (analogRead(6) < DIGITAL_HIGH_THRESHOLD)
		currentState = 1;
	else if (cachedCurrentState >= 1 && cachedCurrentState <= 3)
		currentState = 0;
#else
	else if (PIN_READ(4) == 0)
		currentState = 3;
	else if (analogRead(26) < PICO_DIGITAL_HIGH_THRESHOLD)
		currentState = 2;
	else if (analogRead(27) < PICO_DIGITAL_HIGH_THRESHOLD)
		currentState = 1;
	else if (cachedCurrentState >= 1 && cachedCurrentState <= 3)
		currentState = 0;
#endif
}

void sr_row2sr_isr_legacy()
{
	delayMicroseconds(LINE_WAIT);
	byte cachedCurrentState = currentState;
	if (currentState > 6)
		return;
	else if (PIN_READ(7) == 0)
		currentState = 6;
	else if (analogRead(1) < DIGITAL_HIGH_THRESHOLD)
		currentState = 5;
	else if (analogRead(0) < DIGITAL_HIGH_THRESHOLD)
		currentState = 4;
	else if (cachedCurrentState >= 4 && cachedCurrentState <= 6)
		currentState = 0;
}

void sr_row2sr_isr_vision()
{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	delayMicroseconds(LINE_WAIT);
#else
	for (int i = 0; i < 150*LINE_WAIT; ++i)  // This is trial and error'd.  
		asm volatile("nop\n");    // NOP isn't consistent enough on an optimized Pi Pico
#endif
	byte cachedCurrentState = currentState;
	
	if (currentState > 6)
		return;
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	else if (PIN_READ(6) == 0)
		currentState = 6;
	else if (analogRead(7) < DIGITAL_HIGH_THRESHOLD)
		currentState = 5;
	else if (analogRead(6) < DIGITAL_HIGH_THRESHOLD)
		currentState = 4;
	else if (cachedCurrentState >= 4 && cachedCurrentState <= 6)
		currentState = 0;
#else
	else if (PIN_READ(4) == 0)
		currentState = 6;
	else if (analogRead(26) < PICO_DIGITAL_HIGH_THRESHOLD)
		currentState = 5;
	else if (analogRead(27) < PICO_DIGITAL_HIGH_THRESHOLD)
		currentState = 4;
	else if (cachedCurrentState >= 4 && cachedCurrentState <= 6)
		currentState = 0;
#endif
}

void KeyboardControllerSpy::setup(byte controllerMode, uint8_t cableType)
{	
	this->cableType = cableType;
	this->currentControllerMode = controllerMode;

	currentState = 0;
	lastState = 0xFF;
	
#ifndef DEBUG
	if (currentControllerMode == MODE_NORMAL)
	{
		for (int i = 2; i <= 8; ++i)
			pinMode(i, INPUT_PULLUP);
		
		if (cableType == CABLE_GENESIS)
		{
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
			attachInterrupt(digitalPinToInterrupt(0), row1_isr_vision, FALLING);
			attachInterrupt(digitalPinToInterrupt(1), row2_isr_vision, FALLING);
			attachInterrupt(digitalPinToInterrupt(2), row3_isr_vision, FALLING);
			attachInterrupt(digitalPinToInterrupt(3), row4_isr_vision, FALLING); 
#else
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(2), row1_isr_vision, FALLING);
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(3), row2_isr_vision, FALLING);
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(4), row3_isr_vision, FALLING);
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(5), row4_isr_vision, FALLING);
#endif
		}
		else
		{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(2), row1_isr_legacy, FALLING);
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(3), row2_isr_legacy, FALLING);
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(4), row3_isr_legacy, FALLING);
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(5), row4_isr_legacy, FALLING);
#endif
		}

	}
	else if (currentControllerMode == MODE_STAR_RAIDERS)
	{
		pinMode(A0, INPUT);
		pinMode(A1, INPUT);
		
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
		pinMode(15, OUTPUT);
		digitalWrite(15, HIGH);
		pinMode(14, OUTPUT);
		digitalWrite(14, HIGH);
#endif
		
		if (cableType == CABLE_GENESIS)
		{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(2), sr_row1sr_isr_vision, FALLING);
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(3), sr_row2sr_isr_vision, FALLING);
#else
			attachInterrupt(digitalPinToInterrupt(0), sr_row1sr_isr_vision, FALLING);
			attachInterrupt(digitalPinToInterrupt(1), sr_row2sr_isr_vision, FALLING);
#endif
		}
		else
		{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(2), sr_row1sr_isr_legacy, FALLING);
			attachPinChangeInterrupt(digitalPinToPinChangeInterrupt(3), sr_row2sr_isr_legacy, FALLING);
#endif
		}
	}
	else
	{
#if defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
		pinMode(15, OUTPUT);
		digitalWrite(15, HIGH);
		pinMode(14, OUTPUT);
		digitalWrite(14, HIGH);
#endif
	}
#endif
}

void KeyboardControllerSpy::loop()
{
#ifdef DEBUG
	noInterrupts();
	rawData = 0;
	rawData |= (READ_PORTD(0xFF) >> 2) | (READ_PORTB(0xFF) << 6);
	int analog0 = analogRead(6);
	int analog1 = analogRead(7);
	int analog2 = analogRead(2);
	int analog3 = analogRead(3);
	interrupts();
#else
	if (currentControllerMode == MODE_BIG_BIRD)
	{
		byte bytemask;
		byte digitalPin;
		byte analogPin;
		int digitalThreshold = DIGITAL_HIGH_THRESHOLD;
			
		if (cableType == CABLE_GENESIS)
		{
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
			bytemask = 0b01000000;
			digitalPin = 6;
			analogPin = 7;
#else
			bytemask = 0xFF;
			digitalPin = 4;
			analogPin = 26;
			digitalThreshold = PICO_DIGITAL_HIGH_THRESHOLD;
#endif
		}
		else
		{
			bytemask = 0b10000000;
			digitalPin = 7;
			analogPin = 1;
		}
		noInterrupts();
		byte pin6 = PIN_READ(digitalPin);
		int pin9 = analogRead(analogPin);
		interrupts();
		if ((pin6 & bytemask) == 0)
			currentState = 6;
		else if (pin9 < digitalThreshold)
			currentState = 5;
		else
			currentState = 0;
	}
#endif

#ifdef DEBUG
	//if (rawData != lastRawData)
	//{
	Serial.print(currentState);
	Serial.print("|");
	Serial.print((rawData & 0b0000000000000001) != 0 ? "-" : "1");
	Serial.print((rawData & 0b0000000000000010) != 0 ? "-" : "2");
	Serial.print((rawData & 0b0000000000000100) != 0 ? "-" : "3");
	Serial.print((rawData & 0b0000000000001000) != 0 ? "-" : "4");
	Serial.print((rawData & 0b0000000000010000) != 0 ? "-" : "5");
	Serial.print((rawData & 0b0000000000100000) != 0 ? "-" : "6");
	Serial.print((rawData & 0b0000000001000000) != 0 ? "-" : "7");
	Serial.print("|");
	Serial.print(analog0);
	Serial.print("|");
	Serial.print(analog1);
	Serial.print("|");
	Serial.print(analog2);
	Serial.print("|");
	Serial.println(analog3);
	lastRawData = rawData;
	//}
#else
	if (currentState != lastState)
	{
#ifdef PRETTY_PRINT
		Serial.println(currentState);
#else
		Serial.write(currentState + (byte)65);
		Serial.write("\n");
#endif
		lastState = currentState;
	}
#endif
}

void KeyboardControllerSpy::writeSerial() {}
void KeyboardControllerSpy::debugSerial() {}
void KeyboardControllerSpy::updateState() {}

#else

void KeyboardControllerSpy::setup(byte controllerMode, uint8_t cableType) {}
void KeyboardControllerSpy::loop() {}
void KeyboardControllerSpy::writeSerial() {}
void KeyboardControllerSpy::debugSerial() {}
void KeyboardControllerSpy::updateState() {}

#endif

const char* KeyboardControllerSpy::startupMsg()
{
	if (currentControllerMode == MODE_NORMAL)
		return "Atari Keyboard Controller (MODE_NORMAL)";
	else if (currentControllerMode == MODE_STAR_RAIDERS)
		return "Atari Keyboard Controller (MODE_STAR_RAIDERS)";
	else
		return "Atari Keyboard Controller (MODE_BIG_BIRD)";
}