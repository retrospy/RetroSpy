/*********************************************************
 * 
 *   Sending end of the GameBoy Link cable tester.
 * 
 *   PINOUT:  
 *            D11 to receiving end D11 is Latch
 *            D12 to receiving end D12 is Clock
 *            Vin to receiving end 5V
 *            GND to receiving end GND
 *            
 *********************************************************/
void setup()
{
    pinMode(0, OUTPUT);
    digitalWrite(0, LOW);
    pinMode(1, OUTPUT);
    digitalWrite(1, LOW);
    pinMode(2, OUTPUT);
    digitalWrite(2, LOW);
    pinMode(5, OUTPUT);
    digitalWrite(5, LOW);
    pinMode(7, OUTPUT);
    digitalWrite(7, LOW);
    pinMode(8, OUTPUT);
    digitalWrite(8, LOW);
    
    Serial.begin(115200);
}

void loop()
{
  int r = 0;
  while(true)
  {
   Serial.print("here :");
   Serial.println(r++);
   
  digitalWrite(8, HIGH);
  delay(1);
  
  for(int i = 0; i < 16; ++i)
  {
    digitalWrite(5,  (i & 0b000001000) != 0 ? HIGH : LOW);
    digitalWrite(2,  (i & 0b000000100) != 0 ? HIGH : LOW);
    digitalWrite(1,  (i & 0b000000010) != 0 ? HIGH : LOW);
    digitalWrite(0,  (i & 0b000000001) != 0 ? HIGH : LOW);

    digitalWrite(7, HIGH);
    delay(1);
    digitalWrite(7, LOW);
    delay(1);
  }
  digitalWrite(8, LOW);
  delay(1);
  }
}
