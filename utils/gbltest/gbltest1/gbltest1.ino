/*********************************************************
 * 
 *   Sending end of the GameBoy Link cable tester.
 * 
 *   PINOUT:  
 *            D8 to receiving end D8
 *            D9 to receiving end D9
 *            Vin to receiving end 5V
 *            GND to receiving end GND
 *            
 *********************************************************/
void setup()
{
    for(int i = 2; i < 10; ++i)
    {
      pinMode(i, OUTPUT);
      digitalWrite(i, LOW);
    }
    Serial.begin(115200);
}

void loop()
{
  digitalWrite(8, HIGH);
  delay(1);
  
  for(int i = 0; i < 64; ++i)
  {
    digitalWrite(6, (i & 0b00000001) != 0 ? HIGH : LOW);
    digitalWrite(5, (i & 0b00000010) != 0 ? HIGH : LOW);
    digitalWrite(4, (i & 0b00000100) != 0 ? HIGH : LOW);
    digitalWrite(3, (i & 0b00001000) != 0 ? HIGH : LOW);
    digitalWrite(2, (i & 0b00010000) != 0 ? HIGH : LOW);
    digitalWrite(7, (i & 0b00100000) != 0 ? HIGH : LOW);

    digitalWrite(9, HIGH);
    delay(1);
    digitalWrite(9, LOW);
    delay(1);
  }
  digitalWrite(8, LOW);
}
