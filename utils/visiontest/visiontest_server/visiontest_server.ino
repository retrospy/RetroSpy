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
    for(int i = 2; i < 12; ++i)
    {
      pinMode(i, OUTPUT);
      digitalWrite(i, LOW);
    }
    Serial.begin(115200);
}

void loop()
{
  int r = 0;
  while(true)
  {
   Serial.print("here :");
   Serial.println(r++);
   
  digitalWrite(10, HIGH);
  delay(1);
  
  for(int i = 0; i < 256; ++i)
  {
    //digitalWrite(10, (i & 0b100000000) != 0 ? HIGH : LOW);
    digitalWrite(9,  (i & 0b010000000) != 0 ? HIGH : LOW);
    digitalWrite(8,  (i & 0b001000000) != 0 ? HIGH : LOW);
    digitalWrite(7,  (i & 0b000100000) != 0 ? HIGH : LOW);
    digitalWrite(6,  (i & 0b000010000) != 0 ? HIGH : LOW);
    digitalWrite(5,  (i & 0b000001000) != 0 ? HIGH : LOW);
    digitalWrite(4,  (i & 0b000000100) != 0 ? HIGH : LOW);
    digitalWrite(3,  (i & 0b000000010) != 0 ? HIGH : LOW);
    digitalWrite(2,  (i & 0b000000001) != 0 ? HIGH : LOW);

    digitalWrite(11, HIGH);
    delay(1);
    digitalWrite(11, LOW);
    delay(1);
  }
  digitalWrite(10, LOW);
  delay(1);
  }
}
