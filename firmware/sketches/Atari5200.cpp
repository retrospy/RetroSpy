//
// Atari5200Spy.cpp
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

#include "Atari5200.h"

#if !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(ARDUINO_AVR_NANO_EVERY) && !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)

static volatile int lastVal = 0;
static volatile int currentVal = 0;
static volatile int analogVal = 0;
static volatile int readFlag;

static int nominal_min = 1023;
static int nominal_max = 0;
static int window[3];
static int windowPosition = 0;
static bool filledWindow = false;
static bool lockedCalibration = false;
static volatile int count = 0;

static byte rawData[17];

#if defined(ATARI5200_ADC_INT_HANDLER)
// Interrupt service routine for the ADC completion
ISR(ADC_vect)
{
	// Must read low first
	analogVal = ADCL | (ADCH << 8);

	if ((analogVal < lastVal && (lastVal - analogVal) > 100 && count > 25) || count > 200)
	{
		currentVal = lastVal;
		lastVal = analogVal;
		readFlag = 1;
		count = 0;
	}
	else
	{
		count++;
		lastVal = analogVal;
	}

	// Not needed because free-running mode is enabled.
	// Set ADSC in ADCSRA (0x7A) to start another ADC conversion
	// ADCSRA |= B01000000;
}
#endif

void Atari5200Spy::setup(bool isSecondArduino)
{
	this->isSecondArduino = isSecondArduino;

	for (int i = 2; i <= 11; ++i)
		pinMode(i, INPUT_PULLUP);

	if (!isSecondArduino)
	{
		pinMode(12, OUTPUT);
	}
	else
	{
		pinMode(12, INPUT);
		lockedCalibration = (digitalRead(12) == HIGH);
	}

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

void Atari5200Spy::loop()
{
	if (readFlag == 1)
	{
#ifndef YAXIS
		WAIT_FALLING_EDGE(5);
		asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			);
		rawData[0] = PINB_READ(0);
		rawData[1] = PINB_READ(1);
		rawData[2] = PINB_READ(2);

		WAIT_FALLING_EDGE(4);
		asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			);
		rawData[3] = PINB_READ(0);
		rawData[4] = PINB_READ(1);
		rawData[5] = PINB_READ(2);
		rawData[6] = PINB_READ(3);

		WAIT_FALLING_EDGE(3);
		asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			);
		rawData[7] = PINB_READ(0);
		rawData[8] = PINB_READ(1);
		rawData[9] = PINB_READ(2);
		rawData[10] = PINB_READ(3);

		WAIT_FALLING_EDGE(2);
		asm volatile(MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS MICROSECOND_NOPS
			);
		rawData[11] = PINB_READ(0);
		rawData[12] = PINB_READ(1);
		rawData[13] = PINB_READ(2);
		rawData[14] = PINB_READ(3);

		rawData[15] = PIN_READ(6);
		rawData[16] = PIN_READ(7);
#endif

		if ((!lockedCalibration && currentVal > nominal_min / 2) || (lockedCalibration && currentVal >= nominal_min * .9f && currentVal <= nominal_max * 1.1f))
		{
			window[windowPosition] = currentVal;

			windowPosition += 1;
			windowPosition = (windowPosition % 3);
			if (!filledWindow && windowPosition == 2)
				filledWindow = true;
			int smoothedValue = middleOfThree(window[0], window[1], window[2]);

			if (!lockedCalibration)
			{
				smoothedValue = middleOfThree(window[0], window[1], window[2]);
				if (filledWindow && smoothedValue < nominal_min)
					nominal_min = smoothedValue;
				if (filledWindow && smoothedValue > nominal_max)
					nominal_max = smoothedValue;

				if (!isSecondArduino)
				{
					lockedCalibration = ((rawData[16] == 0) || (rawData[15] == 0));
					if (lockedCalibration)
						digitalWrite(12, HIGH);
				}
				else
				{
					lockedCalibration = (digitalRead(12) == HIGH);
				}
			}
			else
			{
				if (!isSecondArduino)
				{
					if (rawData[14] == 0 && rawData[6] == 0)
					{
						digitalWrite(12, LOW);
						lockedCalibration = false;
						nominal_min = 1023;
						nominal_max = 0;
					}
				}
				else
				{
					if (digitalRead(12) == LOW)
					{
						lockedCalibration = false;
						nominal_min = 1023;
						nominal_max = 0;
					}
				}
			}

#ifdef DEBUG  
			Serial.print((rawData[2] == 0) ? "s" : "-");
			Serial.print((rawData[1] == 0) ? "p" : "-");
			Serial.print((rawData[0] == 0) ? "r" : "-");
			Serial.print((rawData[5] == 0) ? "1" : "-");
			Serial.print((rawData[4] == 0) ? "4" : "-");
			Serial.print((rawData[3] == 0) ? "7" : "-");
			Serial.print((rawData[6] == 0) ? "*" : "-");
			Serial.print((rawData[9] == 0) ? "2" : "-");
			Serial.print((rawData[8] == 0) ? "5" : "-");
			Serial.print((rawData[7] == 0) ? "8" : "-");
			Serial.print((rawData[10] == 0) ? "0" : "-");
			Serial.print((rawData[13] == 0) ? "3" : "-");
			Serial.print((rawData[12] == 0) ? "6" : "-");
			Serial.print((rawData[11] == 0) ? "9" : "-");
			Serial.print((rawData[14] == 0) ? "#" : "-");
			Serial.print((rawData[15] == 0) ? "t" : "-");
			Serial.print((rawData[16] == 0) ? "f" : "-");
			Serial.print("|");
			Serial.print(ScaleInteger(smoothedValue, nominal_min, nominal_max, 0, 255));
			Serial.print("|");
			Serial.print(currentVal);
			Serial.print("|");
			Serial.print(smoothedValue);
			Serial.print("|");
			Serial.print(nominal_min);
			Serial.print("|");
			Serial.print(nominal_max);
			Serial.print("|");
			Serial.print(lockedCalibration);
			Serial.print("\n");
#else
			int sil = ScaleInteger(smoothedValue, nominal_min, nominal_max, 0, 255);
			Serial.write((rawData[2] == 0) ? 0 : 1);
			Serial.write((rawData[1] == 0) ? 0 : 1);
			Serial.write((rawData[0] == 0) ? 0 : 1);
			Serial.write((rawData[5] == 0) ? 0 : 1);
			Serial.write((rawData[9] == 0) ? 0 : 1);
			Serial.write((rawData[13] == 0) ? 0 : 1);
			Serial.write((rawData[4] == 0) ? 0 : 1);
			Serial.write((rawData[8] == 0) ? 0 : 1);
			Serial.write((rawData[12] == 0) ? 0 : 1);
			Serial.write((rawData[3] == 0) ? 0 : 1);
			Serial.write((rawData[7] == 0) ? 0 : 1);
			Serial.write((rawData[11] == 0) ? 0 : 1);
			Serial.write((rawData[6] == 0) ? 0 : 1);
			Serial.write((rawData[10] == 0) ? 0 : 1);
			Serial.write((rawData[14] == 0) ? 0 : 1);
			Serial.write((rawData[15] == 0) ? 0 : 1);
			Serial.write((rawData[16] == 0) ? 0 : 1);
			Serial.write(((sil & 0x0F) << 4));
			Serial.write((sil & 0xF0));
			Serial.write('\n');
#endif
		}
		readFlag = 0;
		delay(5);
	}
}

void Atari5200Spy::writeSerial() {}
void Atari5200Spy::debugSerial() {}
void Atari5200Spy::updateState() {}

#else

void Atari5200Spy::setup(bool isSecondArduino) {}
void Atari5200Spy::loop() {}
void Atari5200Spy::writeSerial() {}
void Atari5200Spy::debugSerial() {}
void Atari5200Spy::updateState() {}

#endif

const char* Atari5200Spy::startupMsg()
{
	if (isSecondArduino)
		return "Atari 5200 (true)";
	else
		return "Atari 5200 (false)";
}