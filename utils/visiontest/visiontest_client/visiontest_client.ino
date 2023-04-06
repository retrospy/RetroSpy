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

#define PIND_READ( pin ) (PIND&(1<<(pin)))
#define PINB_READ( pin ) (PINB&(1<<(pin)))
#define PINB_READ( pin ) (PINB&(1<<(pin)))
#define WAIT_LEADING_EDGEB( pin ) while( PINB_READ(pin) ){} while( !PINB_READ(pin) ){}

void setup()
{
    count = 0;
    for(int i = 2; i < 12; ++i)
    {
      pinMode(i, INPUT_PULLUP);
    }

  PORTC = 0xFF; // Set the pull-ups on the port we use to check operation mode.
  DDRC  = 0x00;

    Serial.begin(115200);
}

void loop()
{
  while(true)
  {
  if ((PINC & 0b00100000) == 0)
  {
    Serial.print("Switch=");
    Serial.println(~PINC & 0b00111111);
  }
  else
  {
  WAIT_LEADING_EDGEB(2);

  //Serial.print("Starting Cycle #");
  //Serial.println(count);

  //noInterrupts();
  for(int i = 0; i < 256; ++i)
  { 

    WAIT_LEADING_EDGEB(3);
    //Serial.println(PIND & 0b11111100);
    //Serial.println(PINB & 0b00000111);
    //int val = (((PINB & 0b00000111) << 6) | ((PIND & 0b11111100) >> 2));
    int val = ((PINB & 0b00000011) << 6) | ((PIND & 0b11111100) >> 2);
  
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
  Serial.print((~PINC & 0b00111111));
  Serial.println("");
  }
  }
}
