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

#define PINB_READ( pin ) (gpio_get(pin + 6))
#define WAIT_LEADING_EDGEB( pin ) while( PINB_READ(pin) ){} while( !PINB_READ(pin) ){}

void setup()
{
    count = 0;
    for(int i = 0; i < 19; ++i)
    {
      pinMode(i, INPUT_PULLUP);
    }

    pinMode(21, OUTPUT);
    pinMode(22, OUTPUT);

    delay(5000);
}

void loop()
{
  while(true)
  {
    bool failure = false;
    WAIT_LEADING_EDGEB(3);
    noInterrupts();
    for(int i = 0; i < 65536; ++i)
    { 

      WAIT_LEADING_EDGEB(4);
  
      int val = (gpio_get_all() & 0x0001FF) | ((gpio_get_all() & 0x07F000) >> 3);
      
      if (val != i)
      {
        interrupts();
        Serial.print("FAILED ");
        Serial.print(i);
        Serial.print("!=");
        Serial.print(val);
        Serial.println(".");
        digitalWrite(21, HIGH);
        digitalWrite(22, LOW);
        failure = true;
      }
    }
  
    interrupts();
    if (!failure)
    {
      Serial.print(1);
      digitalWrite(21, LOW);
      digitalWrite(22, HIGH);
    }
  }
}
