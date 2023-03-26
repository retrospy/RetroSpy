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
    for(int i = 2; i < 12; ++i)
    {
      pinMode(i, INPUT_PULLUP);
    }

    Serial.begin(115200);
}

void loop()
{

  WAIT_LEADING_EDGEB(3);

  //Serial.print("Starting Cycle #");
  //Serial.println(++count);

  noInterrupts();
  for(int i = 0; i < 64; ++i)
  { 
    
    WAIT_LEADING_EDGEB(4);
    int val = 0;
    for(int j = 0; j < 6; ++i)
    {
	    val |= digitalRead(j + 2) == HIGH ? (1 << j) : 0; 
    }
    
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
  for(int j = 0; j < 6; ++i)
      Serial.print(digitalRead(A0+j == HIGH ? "1" : "0");
  Serial.println("");
}
