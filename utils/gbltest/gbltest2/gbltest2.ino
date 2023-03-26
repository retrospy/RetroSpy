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

#define PINB_READ( pin ) (PINB&(1<<(pin)))
#define WAIT_LEADING_EDGEB( pin ) while( PINB_READ(pin) ); while( !PINB_READ(pin) )

void setup()
{
    count = 0;
    for(int i = 2; i < 10; ++i)
    {
      pinMode(i, INPUT_PULLUP);
    }

    Serial.begin(115200);
}

void loop()
{

  //WAIT_LEADING_EDGEB(0);

  //Serial.print("Starting Cycle #");
  //Serial.println(++count);

  noInterrupts();
  for(int i = 0; i < 64; ++i)
  { 
    
    //WAIT_LEADING_EDGEB(1);
    byte val = 0;
    for(int i = 0; i < 6; ++i)
    {
      Serial.println(i);
      val |= digitalRead(i+2) == HIGH ? (1 << i) : 0; 
    }
    
//    if (val != i)
//    {
//      interrupts();
//      Serial.print("Expected: ");
//      Serial.print(i);
//      Serial.print("  Got: ");
//      Serial.println(val);
//      Serial.print("Cycle #");
//      Serial.print(++count);
//      Serial.println(" FAILED");
//      while(true);
//    }
    //else
    {
      interrupts();
      Serial.print(i);
      Serial.print(" equal to ");
      Serial.println(val);
      noInterrupts();
    }
  }
  interrupts();
  
  Serial.print("Cycle #");
  Serial.print(++count);
  Serial.println(" PASSED");

}
