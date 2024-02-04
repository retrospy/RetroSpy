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

    pinMode(14, OUTPUT);
    pinMode(15, OUTPUT);

    digitalWrite(14, HIGH);
    digitalWrite(15, HIGH);

  for(int i = 16; i < 22; ++i)
  {
     pinMode(i, INPUT_PULLUP);
  }
    Serial.begin(115200);

    delay(5000);
}

static bool failure = true;

void loop1()
{
  delay(1);
  
  byte switches = 0;
  switches = (~gpio_get_all() & 0x3f0000) >> 16;

  Serial.print(!failure);
  Serial.print(",");
  Serial.println(switches);
}

void loop()
{
  while(true)
  {
    //byte switches = 0;
   
    WAIT_LEADING_EDGEB(3);

    bool didFail = false;
    noInterrupts();
    for(int i = 0; i < 2048; ++i)
    { 
      WAIT_LEADING_EDGEB(4);
  
      //switches = (~gpio_get_all() & 0x3f0000) >> 16;
    
      //Maybe???  12 and 13 might need analog read and some translation logic
      int val = (gpio_get_all() & 0x0001FF);
      if (analogRead(26) > 500)
        val |= 512;
      if (analogRead(27) > 500)
        val |= 1024;
    
      if (val != i)
      {
        interrupts();
        
        //Serial.print(0);
        //Serial.print(",");
        //Serial.println(switches);
        failure = true;
        didFail = true;
      }
    }
    interrupts();

    if (!didFail)
    {
      failure = false;
      //Serial.print(1);
      //Serial.print(",");
      //Serial.println(switches);
    }
  }
}
