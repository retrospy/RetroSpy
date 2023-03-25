//
// AtariPaddlesSpy.cpp
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

#include "AtariPaddles.h"

#if !(defined(__arm__) && defined(CORE_TEENSY)) && !defined(ARDUINO_AVR_NANO_EVERY) && !defined(RASPBERRYPI_PICO)  && !defined(ARDUINO_RASPBERRY_PI_PICO)

static int nominal_min = 1024;
static int nominal_max = 0;
static volatile int lastVal = 0;
static volatile int currentVal = 0;
static volatile int analogVal = 0;
static volatile int readFlag;

static int window[3];
static int windowPosition = 0;

#if defined(ATARIPADDLES_ADC_INT_HANDLER)
ISR(ADC_vect)
{
	
	// Must read low first
	analogVal = ADCL | (ADCH << 8);

	if (analogVal < lastVal && (lastVal - analogVal) > 20)
	{
		currentVal = lastVal;
		lastVal = analogVal;
		readFlag = 1;
	}
	else
	{
		lastVal = analogVal;
	}
 
	// Not needed because free-running mode is enabled.
	// Set ADSC in ADCSRA (0x7A) to start another ADC conversion
	// ADCSRA |= B01000000;
}
#endif

void AtariPaddlesSpy::setup()
{
	for (int i = 2; i <= 8; ++i)
		pinMode(i, INPUT_PULLUP);

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

void AtariPaddlesSpy::loop()
{
	if (readFlag == 1)
	{

		byte pins = 0;
		pins |= (PIND >> 2);
      
		byte fire2 = ((pins & 0b0000000000001000) == 0);
		window[windowPosition] = currentVal;
		windowPosition += 1;
		windowPosition = (windowPosition % 3);

		int smoothedValue = middleOfThree(window[0], window[1], window[2]);
		if (smoothedValue > nominal_max)
			nominal_max = smoothedValue;
		if (smoothedValue != 0 && smoothedValue < nominal_min)
			nominal_min = smoothedValue; 
  
#ifdef DEBUG
		Serial.print("-");
		Serial.print(fire2 ? "4" : "-");
		Serial.print("|");
		Serial.print(ScaleInteger(smoothedValue, nominal_min, nominal_max, 0, 255));
		Serial.print("|");
		Serial.print(nominal_min);
		Serial.print("|");
		Serial.print(nominal_max);
		Serial.print("\n");
#else
		int sil = ScaleInteger(smoothedValue, nominal_min, nominal_max, 0, 255);
		Serial.write(0);
		Serial.write(fire2);
		Serial.write(sil);
		Serial.write(0);
		Serial.write(5);
		Serial.write(11);
		Serial.write('\n');
#endif
		readFlag = 0;
		delay(5);	
	}
}

void AtariPaddlesSpy::writeSerial()
{
	
}

void AtariPaddlesSpy::debugSerial()
{
	
}

void AtariPaddlesSpy::updateState()
{
	
}

#else

void AtariPaddlesSpy::setup() {}
void AtariPaddlesSpy::loop() {}
void AtariPaddlesSpy::writeSerial() {}
void AtariPaddlesSpy::debugSerial() {}
void AtariPaddlesSpy::updateState() {}

#endif

const char* AtariPaddlesSpy::startupMsg()
{
	return "Atari Paddles";
}