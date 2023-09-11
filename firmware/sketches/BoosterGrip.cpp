//
// BoosterGrip.cpp
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

#include "BoosterGrip.h"

#if !(defined(__arm__) && defined(CORE_TEENSY))

void BoosterGripSpy::setup(uint8_t cableType) {
	this->cableType = cableType;
	setup();
}

void BoosterGripSpy::setup() {
	
	switch (cableType) {
	case CABLE_SMS:
		inputPins[0] = BOOSTER_GRIP_INPUT_PIN_0;
		inputPins[1] = BOOSTER_GRIP_INPUT_PIN_1;
		inputPins[2] = BOOSTER_GRIP_INPUT_PIN_2;
		inputPins[3] = BOOSTER_GRIP_INPUT_PIN_3;
		inputPins[4] = BOOSTER_GRIP_INPUT_PIN_4;
		inputPins[5] = BOOSTER_GRIP_INPUT_PIN_5;
		inputPins[6] = BOOSTER_GRIP_INPUT_PIN_6;
		break;
	case CABLE_GENESIS:
		inputPins[0] = BOOSTER_GRIP_ONGEN_INPUT_PIN_0;
		inputPins[1] = BOOSTER_GRIP_ONGEN_INPUT_PIN_1;
		inputPins[2] = BOOSTER_GRIP_ONGEN_INPUT_PIN_2;
		inputPins[3] = BOOSTER_GRIP_ONGEN_INPUT_PIN_3;
		inputPins[4] = BOOSTER_GRIP_ONGEN_INPUT_PIN_4;
		inputPins[5] = BOOSTER_GRIP_ONGEN_INPUT_PIN_5;
		inputPins[6] = BOOSTER_GRIP_ONGEN_INPUT_PIN_6;
		break;
	}

	// Setup input pins
	for (byte i = 0; i < BG_INPUT_PINS; i++) {
		if (i == 4 || i == 6) {
			pinMode(inputPins[i], INPUT);
		}
		else {
			pinMode(inputPins[i], INPUT_PULLUP);
		}
	}

	lastReadTime = millis();
}

void BoosterGripSpy::loop() {
	if (max(millis() - lastReadTime, 0U) < BG_READ_DELAY_MS)
	{
		// Not enough time has elapsed, return
		return;
	}
	currentState = 0;
	noInterrupts();
	updateState();
	interrupts();
#if !defined(DEBUG)
	writeSerial();
#else
	debugSerial();
#endif

	lastReadTime = millis();
}

void BoosterGripSpy::updateState() {
	// Read input pins for Up, Down, Left, Right, 1, 2, 3
	if (digitalRead(inputPins[0]) == LOW) { currentState |= BG_BTN_UP; }
	if (digitalRead(inputPins[1]) == LOW) { currentState |= BG_BTN_DOWN; }
	if (digitalRead(inputPins[2]) == LOW) { currentState |= BG_BTN_LEFT; }
	if (digitalRead(inputPins[3]) == LOW) { currentState |= BG_BTN_RIGHT; }
	if (digitalRead(inputPins[4]) == HIGH) { currentState |= BG_BTN_3; }
	if (digitalRead(inputPins[5]) == LOW) { currentState |= BG_BTN_1; }
	if (digitalRead(inputPins[6]) == HIGH) { currentState |= BG_BTN_2; }
}

void BoosterGripSpy::writeSerial() {
	for (unsigned char i = 0; i < 7; ++i) {
		Serial.write(currentState & (1 << i) ? ONE : ZERO);
	}
	Serial.write(SPLIT);
}

void BoosterGripSpy::debugSerial() {
	if (currentState != lastState) {
		Serial.print((currentState & BG_BTN_UP) ? "U" : "0");
		Serial.print((currentState & BG_BTN_DOWN) ? "D" : "0");
		Serial.print((currentState & BG_BTN_LEFT) ? "L" : "0");
		Serial.print((currentState & BG_BTN_RIGHT) ? "R" : "0");
		Serial.print((currentState & BG_BTN_1) ? "1" : "0");
		Serial.print((currentState & BG_BTN_2) ? "2" : "0");
		Serial.println((currentState & BG_BTN_3) ? "3" : "0");
		lastState = currentState;
	}
}

#else
void BoosterGripSpy::setup() {}

void BoosterGripSpy::loop() {}

void BoosterGripSpy::writeSerial() {}

void BoosterGripSpy::debugSerial() {}

void BoosterGripSpy::updateState() {}
#endif

const char* BoosterGripSpy::startupMsg()
{
	return "Booster Grip";
}