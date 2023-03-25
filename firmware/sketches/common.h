//
// common.h
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

#include "Arduino.h"

#if  defined(__arm__) && defined(CORE_TEENSY) && (defined(ARDUINO_TEENSY40) || defined(ARDUINO_TEENSY41))
#include "config_teensy4.h"
#elif defined(__arm__) && defined(CORE_TEENSY) && defined(ARDUINO_TEENSY35)
#include "config_teensy.h"
#elif defined(ARDUINO_AVR_NANO_EVERY)
#include "config_every.h"
#elif defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
#include "config_pico.h"
#else
#include "config_arduino.h"
#endif

#ifndef VIDEO_OUTPUT_TYPE
#define VIDEO_OUTPUT_TYPE
enum VideoOutputType {
	VIDEO_PAL = 1,
	VIDEO_NTSC = 2,
};
#endif

// Vision Hardware Configurations
//#define RS_VISION
//#define RS_VISION_ULTRA
//#define RS_VISION_CDI
//#define RS_VISION_COLECOVISION

#ifdef RS_VISION
#define TP_TIMERONE
#define TP_PINCHANGEINTERRUPT
#endif

#ifdef RS_VISION_ULTRA
#ifdef N64_PIN
#undef N64_PIN
#endif
#define N64_PIN 0
#endif

#ifdef RS_VISION_CDI
#define TP_IRLIB2
#endif

#ifdef RS_VISION_COLECOVISION
#define TP_PINCHANGEINTERRUPT
#define COLECOVISION_ROLLER_TIMER_INT_HANDLER
#endif

// Uncomment these to enable 3rd party libraries once installed
//#define TP_IRREMOTE             // Used by MODE_CDTV_WIRELESS
//#define TP_IRLIB2               // Used by MODE_CDI
// Used by MODE_PIPPIN & MODE_CDTV_WIRED
//#define TP_TIMERONE             
// Used by MODE_COLECOVISION, MODE_DRIVING_CONTROLLER & MODE_KEYBOARD_CONTROLLER
//#define TP_PINCHANGEINTERRUPT

// Uncomment these out to enable the necessary ADC interrupt handler.
// They cannot co-exist when linked even when not active
//#define AMIGA_ANALOG_ADC_INT_HANDLER
//#define ATARI5200_ADC_INT_HANDLER
//#define ATARIPADDLES_ADC_INT_HANDLER
//#define COLECOVISION_ROLLER_TIMER_INT_HANDLER

	
// Uncomment this for serial debugging output
//#define DEBUG

#define N64_BITCOUNT		    32
#define SNES_BITCOUNT       16
#define SNES_BITCOUNT_EXT   32
#define NES_BITCOUNT         8
#define GC_BITCOUNT			    64
#define GC_PREFIX           25
#define ThreeDO_BITCOUNT	  32
#define PCFX_BITCOUNT		    16
#define CD32_BITCOUNT		     7

#define PIN_READ PIND_READ

#define WAIT_FALLING_EDGE( pin ) while( !PIN_READ(pin) ); while( PIN_READ(pin) );
#define WAIT_LEADING_EDGE( pin ) while( PIN_READ(pin) ); while( !PIN_READ(pin) );

#define WAIT_FALLING_EDGEB( pin ) while( !PINB_READ(pin) ); while( PINB_READ(pin) );
#define WAIT_LEADING_EDGEB( pin ) while( PINB_READ(pin) ); while( !PINB_READ(pin) );

#define ZERO  ((uint8_t)0)  // Use a byte value of 0x00 to represent a bit with value 0.
#define ONE   '1'  // Use an ASCII one to represent a bit with value 1.  This makes Arduino debugging easier.
#define SPLIT '\n'  // Use a new-line character to split up the controller state packets.

void common_pin_setup();
void read_shiftRegister_2wire(unsigned char rawData[], unsigned char latch, unsigned char data, unsigned char longWait, unsigned char bits);
void sendRawData(unsigned char rawControllerData[], unsigned char first, unsigned char count);
void sendRawDataDebug(unsigned char rawControllerData[], unsigned char first, unsigned char count);
int ScaleInteger(float oldValue, float oldMin, float oldMax, float newMin, float newMax);
int middleOfThree(int a, int b, int c);
