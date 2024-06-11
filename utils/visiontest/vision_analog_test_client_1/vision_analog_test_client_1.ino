/*********************************************************
 * 
 *   Receiving end of the GameBoy Link cable tester.
 * 
 *   PINOUT:  
 *            D8 to sending end D8
 *            D9 to sending end D9
 *            5V to sending end Vin
 *            GND to sending end GND
 *            
 *********************************************************/
#include "digitalWriteFast.h"

static int count = 0;

#define PIND_READ( pin ) (PIND&(1<<(pin)))
#define PINB_READ( pin ) (PINB&(1<<(pin-8)))
#define WAIT_LEADING_EDGEB( pin ) while( PINB_READ(pin) ){} while( !PINB_READ(pin) ){}

void setup()
{
  Serial.println("here1");
    count = 0;
    for(int i = 2; i < 13; ++i)
    {
      pinMode(i, INPUT_PULLUP);
    }

    for(int i = A1; i < A6; ++i)
      pinMode(i, INPUT_PULLUP);

    pinMode(A0, INPUT);

    Serial.begin(115200);
Serial.println("here2");
    while(!Serial);
}

static byte switches;

static bool failure = true;

void loop()
{
  while(true)
  {
    Serial.println("waiting for latch");
    pinMode(12, OUTPUT);
    digitalWriteFast(12, HIGH);
    WAIT_LEADING_EDGEB(11);
    digitalWriteFast(12, LOW);
    digitalWriteFast(12, HIGH);
    pinMode(12, INPUT_PULLUP);
    
    bool didFail = false;
    //noInterrupts();
    for(int i = 0; i < 2048; ++i)
    { 
      Serial.println("waiting on clock");
      WAIT_LEADING_EDGEB(12);

      switches = 0;
      switches = (~PINC >> 1) & 0x1F;
   int val = 0;
         if (i >= 512)
        val = i - 512;
      else
        val = i;

      //int val = (PIND >> 2) | ((PINB & 0x07) << 6);
      
      int analogval = analogRead(A0);
      if (analogval > 500)
      {
        val |= 512;
      }

       Serial.print(i);
       Serial.print(",");
       Serial.print(val);
       Serial.print(",");
       Serial.println(switches);
    
      if (val != i)
      {
        interrupts();
        
        Serial.print(0);
        Serial.print(",");
        Serial.println(switches);
        failure = true;
        didFail = true;
      }
    }
    interrupts();

    if (!didFail)
    {
      failure = false;
      Serial.print(1);
      Serial.print(",");
      Serial.println(switches);
    }
  }
}
