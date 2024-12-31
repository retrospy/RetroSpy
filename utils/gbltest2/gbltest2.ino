
#define PIN1_SRC 4
#define PIN2_SRC 3
#define PIN3_SRC 2
#define PIN4_SRC 1
#define PIN5_SRC 0
#define PIN6_SRC 5

#define PIN1_RECV 6

#define LATCH_SRC 12
#define LATCH_RECV 13
#define CLOCK_SRC 14
#define CLOCK_RECV 15

#define LED1 16
#define LED2 17

void setup() {
  
    for(int i = 0; i < 6; ++i)
    {
      pinMode(i, OUTPUT);
      digitalWrite(i, LOW);
    }
    pinMode(LATCH_SRC, OUTPUT);
    digitalWrite(LATCH_SRC, LOW);
    pinMode(CLOCK_SRC, OUTPUT);
    digitalWrite(CLOCK_SRC, LOW);
}

void setup1()
{
    for(int i = PIN1_RECV; i < PIN1_RECV + 6; ++i)
    {
      pinMode(i, INPUT_PULLUP);
    }
    pinMode(LATCH_RECV, INPUT_PULLUP);
    pinMode(CLOCK_RECV, INPUT_PULLUP);

    pinMode(LED1, OUTPUT);
    digitalWrite(LED1, LOW);
    pinMode(LED2, OUTPUT);
    digitalWrite(LED2, LOW);

    Serial.begin(115200);
}

void loop() 
{
  digitalWrite(LATCH_SRC, HIGH);
  delay(1);
  
  for(int i = 0; i < 64; ++i)
  {
    digitalWrite(PIN1_SRC, (i & 0b00000001) != 0 ? HIGH : LOW);
    digitalWrite(PIN2_SRC, (i & 0b00000010) != 0 ? HIGH : LOW);
    digitalWrite(PIN3_SRC, (i & 0b00000100) != 0 ? HIGH : LOW);
    digitalWrite(PIN4_SRC, (i & 0b00001000) != 0 ? HIGH : LOW);
    digitalWrite(PIN5_SRC, (i & 0b00010000) != 0 ? HIGH : LOW);
    digitalWrite(PIN6_SRC, (i & 0b00100000) != 0 ? HIGH : LOW);

    digitalWrite(CLOCK_SRC, HIGH);
    delay(1);
    digitalWrite(CLOCK_SRC, LOW);
    delay(1);
  }
  digitalWrite(LATCH_SRC, LOW);
}

#define WAIT_LEADING_EDGE( pin ) while( gpio_get(pin) ); while( !gpio_get(pin) )

int count = 0;

void loop1()
{
  WAIT_LEADING_EDGE(LATCH_RECV);

   bool foundError = false;
   
  noInterrupts();
  for(int i = 0; i < 64; ++i)
  { 
    WAIT_LEADING_EDGE(CLOCK_RECV);
    byte val = 0;
    for(int i = 0; i < 6; ++i)
    {
      //Serial.println(i);
      val |= digitalRead(i+6) == HIGH ? (1 << i) : 0; 
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
      digitalWrite(LED2, HIGH);
      digitalWrite(LED1, LOW);
      foundError = true;
    }
    else
    {
      interrupts();
      Serial.print(i);
      Serial.print(" equal to ");
      Serial.println(val);
      noInterrupts();
    }
  }
  interrupts();

  if (!foundError)
  {
    digitalWrite(LED2, LOW);
    digitalWrite(LED1, HIGH);
    Serial.print("Cycle #");
    Serial.print(++count);
    Serial.println(" PASSED");
  }
}
