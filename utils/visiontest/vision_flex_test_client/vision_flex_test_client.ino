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
    for(int i = 0; i < 12; ++i)
    {
      pinMode(i, INPUT_PULLUP);
    }

  for(int i = 16; i < 22; ++i)
  {
     pinMode(i, INPUT_PULLUP);
  }
    Serial.begin(115200);

    delay(5000);
}

void loop()
{
  while(true)
  {
    byte switches = 0;
  //if (gpio_get(21) == LOW)
  //{
  //  Serial.print("Switch=");
  //  byte switches = (~gpio_get_all() & 0x3f0000) >> 16;
  //  Serial.println(switches);
  //}
  //else
  {
  WAIT_LEADING_EDGEB(2);

  //Serial.print("Starting Cycle #");
  //Serial.println(count);

  noInterrupts();
  for(int i = 0; i < 256; ++i)
  { 
    WAIT_LEADING_EDGEB(3);

    switches = (~gpio_get_all() & 0x3f0000) >> 16;
    //Serial.println(switches);

    //Serial.println(PIND & 0b11111100);
    //Serial.println(PINB & 0b00000111);
    //int val = (((PINB & 0b00000111) << 6) | ((PIND & 0b11111100) >> 2));
    int val = gpio_get_all() & 0xFF;
  
    if (val != i)
    {
      interrupts();
      //Serial.print("Expected: ");
      //Serial.print(i);
      //Serial.print("  Got: ");
      //Serial.println(val);
      //Serial.print("Cycle #");
      //Serial.print(++count);
      //Serial.println(" FAILED");
      Serial.print(0);
      Serial.print(",");
      Serial.println(switches);
      //return;
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
  
  //Serial.print("Cycle #");
  //Serial.print(++count);
  //Serial.print(" PASSED, Switch=");

      Serial.print(1);
      Serial.print(",");
      Serial.println(switches);

  }
  }
}
