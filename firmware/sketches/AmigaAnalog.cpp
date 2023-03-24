//
// AmigaAnalog.cpp
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

#include "AmigaAnalog.h"

#if !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(ARDUINO_AVR_NANO_EVERY) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)

static volatile int lastVal = 0;
static volatile int analogVal = 0;
static volatile int readFlag = 0;
static volatile int Count = 0;
static volatile int CapturedCount = 0;
static volatile bool CrossedThreshold = false;

static int window[3];
static int windowPosition = 0;
static bool filledWindow = false;
static bool lockedCalibration = false;
static int nominal_min = 1023;
static int nominal_max = 0;

#if defined(AMIGA_ANALOG_ADC_INT_HANDLER)
// Interrupt service routine for the ADC completion
ISR(ADC_vect)
{
	// Must read low first
	analogVal = ADCL | (ADCH << 8);

	if (CrossedThreshold && analogVal > 925)
	{
		CrossedThreshold = false;
		CapturedCount = Count;
		readFlag = 1;
	}

	if (analogVal < lastVal && (lastVal - analogVal) > 20)
	{
		CrossedThreshold = true;
		lastVal = analogVal;
		Count = 0;
	}
	else
	{
		Count++;
		lastVal = analogVal;
	}

	// Not needed because free-running mode is enabled.
	// Set ADSC in ADCSRA (0x7A) to start another ADC conversion
	// ADCSRA |= B01000000;
}
#endif

void AmigaAnalogSpy::setup(bool isSecondArduino) 
{
	this->isSecondArduino = isSecondArduino;

	for (int i = 2; i <= 8; ++i)
		pinMode(i, INPUT_PULLUP);

	if (!isSecondArduino)
		pinMode(12, OUTPUT);
	else
		pinMode(12, INPUT);
		lockedCalibration = (digitalRead(12) == HIGH);

	windowPosition = 0;

	// clear ADLAR in ADMUX (0x7C) to right-adjust the result
	// ADCL will contain lower 8 bits, ADCH upper 2 (in last two bits)
	ADMUX &= B11011111;

	// Set REFS1..0 in ADMUX (0x7C) to change reference voltage to the
	// proper source (01)
	ADMUX |= B01000000;

	// Clear MUX3..0 in ADMUX (0x7C) in preparation for setting the analog
	// input
	ADMUX &= B11110000;

	// Set MUX3..0 in ADMUX (0x7C) to read from AD8 (Internal temp)
	// Do not set above 15! You will overrun other parts of ADMUX. A full
	// list of possible inputs is available in Table 24-4 of the ATMega328
	// datasheet
	ADMUX |= 0;
	// ADMUX |= B00001000; // Binary equivalent

	// Set ADEN in ADCSRA (0x7A) to enable the ADC.
	// Note, this instruction takes 12 ADC clocks to execute
	ADCSRA |= B10000000;

	// Set ADATE in ADCSRA (0x7A) to enable auto-triggering.
	ADCSRA |= B00100000;

	// Clear ADTS2..0 in ADCSRB (0x7B) to set trigger mode to free running.
	// This means that as soon as an ADC has finished, the next will be
	// immediately started.
	ADCSRB &= B11111000;

	// Set the Prescaler to 128 (16000KHz/128 = 125KHz)
	// Above 200KHz 10-bit results are not reliable.
	ADCSRA |= B00000111;

	// Set ADIE in ADCSRA (0x7A) to enable the ADC interrupt.
	// Without this, the internal interrupt will not trigger.
	ADCSRA |= B00001000;

	// Enable global interrupts
	// AVR macro included in <avr/interrupts.h>, which the Arduino IDE
	// supplies by default.
	sei();

	// Kick off the first ADC
	readFlag = 0;
	// Set ADSC in ADCSRA (0x7A) to start the ADC conversion
	ADCSRA |= B01000000;

}

void AmigaAnalogSpy::loop() 
{
	if (readFlag == 1)
	{
		byte pins = 0;
		pins |= (PIND >> 2);

		byte topButton1 = ((pins & 0b00000001) == 0);
		byte topButton2 = ((pins & 0b00000010) == 0);
		byte triggerButton = ((pins & 0b00000100) == 0);
		byte thumbButton = ((pins & 0b00001000) == 0);

		window[windowPosition] = CapturedCount;
		windowPosition += 1;
		windowPosition = (windowPosition % 3);
		if (!filledWindow && windowPosition == 2)
			filledWindow = true;
		int smoothedValue = middleOfThree(window[0], window[1], window[2]);

		if (!lockedCalibration)
		{
			if (filledWindow && smoothedValue < nominal_min)
				nominal_min = smoothedValue;
			if (filledWindow && smoothedValue > nominal_max)
				nominal_max = smoothedValue;

			if (!isSecondArduino)
			{
				lockedCalibration = (topButton1 || topButton2 || triggerButton || thumbButton);
				if (lockedCalibration)
					digitalWrite(12, HIGH);
			}
			else
				lockedCalibration = (digitalRead(12) == HIGH);
		}
#ifdef DEBUG
		Serial.print(CapturedCount);
		Serial.print("|");
		Serial.print(smoothedValue);
		Serial.print("|");
		Serial.print(topButton1 ? "1" : "-");
		Serial.print(topButton2 ? "2" : "-");
		Serial.print(triggerButton ? "3" : "-");
		Serial.print(thumbButton ? "4" : "-");
		Serial.print("|");
		Serial.print(ScaleInteger(smoothedValue, nominal_min, nominal_max, 0, 30));
		Serial.print("\n");
#else
		int sil = ScaleInteger(smoothedValue, nominal_min, nominal_max, 0, 30);
		Serial.write(topButton1 ? 1 : 0);
		Serial.write(topButton2 ? 1 : 0);
		Serial.write(triggerButton ? 1 : 0);
		Serial.write(thumbButton ? 1 : 0);
		Serial.write(((sil & 0x0F) << 4));
		Serial.write((sil & 0xF0));
		Serial.print("\n");
#endif
		readFlag = 0;
	}
}

void AmigaAnalogSpy::writeSerial() {}
void AmigaAnalogSpy::debugSerial() {}
void AmigaAnalogSpy::updateState() {}

#else

void AmigaAnalogSpy::setup(bool isSecondArduino) {}
void AmigaAnalogSpy::loop() {}
void AmigaAnalogSpy::writeSerial() {}
void AmigaAnalogSpy::debugSerial() {}
void AmigaAnalogSpy::updateState() {}

#endif

const char* AmigaAnalogSpy::startupMsg()
{
	if (isSecondArduino)
		return "Amiga Analog (true)";
	else
		return "Amiga Analog (false)";
}