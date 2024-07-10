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
#define PINB_READ( pin ) (PINB&(1<<(pin)))
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

    while(!Serial);
}

static byte switches;

void loop()
{
  while(true)
  {
    pinMode(12, OUTPUT);
    digitalWriteFast(12, LOW);
    WAIT_LEADING_EDGEB(3);
    digitalWriteFast(12, HIGH);
    pinMode(12, INPUT);
    
    bool didFail = false;
    
    //noInterrupts();
    for(int i = 0; i < 65536; ++i)
    { 
      if (i != 0)
      {
        interrupts();
        Serial.print(didFail ? 0 : 1);
        Serial.print(",");
        switches = 0;
        switches = (~PINC >> 1) & 0x1F;
        Serial.println(switches);
      }
      
      //noInterrupts();
      int val =  0;
      WAIT_LEADING_EDGEB(4);

      if ((i % 2048) >= 512)
        val = (i % 2048) - 512;
      else
        val = i % 2048;

      val = (PIND >> 2) | ((PINB & 0x07) << 6);

      int analogval = analogRead(A0);
      if (analogval > 500)
      { 
        val |= 512;
      }

      if ((val & 1023) != ((i % 2048) & 1023))
      {
        interrupts();  
        Serial.print(0);
        Serial.print(",");
        switches = 0;
        switches = (~PINC >> 1) & 0x1F;
        Serial.println(switches);
        didFail = true;
      }
    }
    interrupts();

    if (!didFail)
    {
      Serial.print(1);
      Serial.print(",");
      switches = 0;
      switches = (~PINC >> 1) & 0x1F;
      Serial.println(switches);
    }
  }
}
