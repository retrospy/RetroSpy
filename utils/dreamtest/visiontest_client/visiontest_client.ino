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
static int count = 0;

#define PIN_READ( pin ) digitalReadFast(pin)
#define WAIT_LEADING_EDGE( pin ) while( PIN_READ(pin) ){} while( !PIN_READ(pin) ){}

void setup()
{
    count = 0;
    pinMode(0, INPUT_PULLUP);
    pinMode(1, INPUT_PULLUP);
    pinMode(2, INPUT_PULLUP);
    pinMode(5, INPUT_PULLUP);
    pinMode(7, INPUT_PULLUP);
    pinMode(8, INPUT_PULLUP);
    
    for(int i = 13; i <=18; ++i)
    {
      pinMode(i, INPUT_PULLUP);
    }

    Serial.begin(115200);
}

void loop()
{
  while(true)
  {
      byte switchVal = 0x00;
  
      for (int i = 0; i < 6; ++i)
      {
        if (digitalReadFast(i + 13) == LOW)
          switchVal |= (1 << i);
      }

      if ((switchVal & 0b00100000) == 0)
      {
        Serial.print("Switch=");
        Serial.println(switchVal & 0b00111111);
      }
      else
      {
        WAIT_LEADING_EDGE(8);
        
        //Serial.print("Starting Cycle #");
        //Serial.println(count);
      
        noInterrupts();
        for(int i = 0; i < 16; ++i)
        { 
      
          WAIT_LEADING_EDGE(7);
          byte val = 0x00;
          val |= digitalReadFast(0) == HIGH ? 0x01 : 0x00;
          val |= digitalReadFast(1) == HIGH ? 0x02 : 0x00;
          val |= digitalReadFast(2) == HIGH ? 0x04 : 0x00;
          val |= digitalReadFast(5) == HIGH ? 0x08 : 0x00;
          
          if (val != i)
          {
            interrupts();
            Serial.print("Expected: ");
            Serial.print(i);
            Serial.print("  Got: ");
            Serial.println(val);
            Serial.print("Cycle #");
            Serial.print(++count);
            Serial.println(" FAILED");
            while(true);
          }
      //    else
      //    {
      //      interrupts();
      //      Serial.print(i);
      //      Serial.print(" equal to ");
      //      Serial.println(val);
      //      noInterrupts();
      //    }
        }
        interrupts();
        
        Serial.print("Cycle #");
        Serial.print(++count);
        Serial.print(" PASSED, Switch=");
        Serial.print(switchVal);
        Serial.println("");

        }

        delay(100);
    }
}
