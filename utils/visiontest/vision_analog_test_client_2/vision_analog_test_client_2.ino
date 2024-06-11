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
#define WAIT_LEADING_EDGED( pin ) while( PIND_READ(pin) ){} while( !PIND_READ(pin) ){}
#define WAIT_LEADING_EDGEB( pin ) while( PINB_READ(pin) ){} while( !PINB_READ(pin) ){}

void setup()
{
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

static bool failure = true;
static byte switches;

void loop()
{
  while(true)
  {
    Serial.println("waiting for latch");
    pinMode(12, INPUT);
    WAIT_LEADING_EDGEB(12);
    pinMode(12, OUTPUT);
    digitalWriteFast(12, HIGH);
    
    bool didFail = false;
    //noInterrupts();
    for(int i = 0; i < 2048; ++i)
    { 
      Serial.println("waiting on clock");
      int val = 0;
      WAIT_LEADING_EDGED(5);
      digitalWriteFast(12, LOW);
      digitalWriteFast(12, HIGH);
      
      switches = 0;
      switches = (~PINC >> 1) & 0x1F;
  
      if (i >= 1024)
        val = i - 1024;
      else
        val = i;

      int analogval = analogRead(A0);
      if (analogval > 500)
      { 
        val |= 1024;
      }


       Serial.print(i);
       Serial.print(",");
       Serial.print(val);
       Serial.print(",");
       Serial.println(switches);
    
      if ((val & 1024) != (i & 1024))
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
