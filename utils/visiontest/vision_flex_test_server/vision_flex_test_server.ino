/*********************************************************

     Sending end of the GameBoy Link cable tester.

     PINOUT:
              D11 to receiving end D11 is Latch
              D12 to receiving end D12 is Clock
              Vin to receiving end 5V
              GND to receiving end GND

 *********************************************************/
void setup()
{
  for (int i = 0; i < 19; ++i)
  {
	if (i != 11)
	{
		pinMode(i, OUTPUT);
		digitalWrite(i, LOW);
	}
  }

  pinMode(11, INPUT_PULLUP);
  pinMode(19, INPUT_PULLUP);
  pinMode(20, INPUT_PULLUP);
  pinMode(26, INPUT_PULLUP);
  pinMode(27, INPUT_PULLUP);  

  pinMode(28, OUTPUT);
  digitalWrite(28, LOW);
  
  pinMode(21, OUTPUT);
  pinMode(22, OUTPUT);
  digitalWrite(21, HIGH);
  digitalWrite(22, LOW);
  
  Serial.begin(115200);
}

bool slowMode = false;

void loop()
{
  int r = 0;
  while (true)
  {
  	if (digitalRead(11) == LOW)
  		slowMode = true;

    digitalWrite(9, HIGH);
    delay(1);
    
    for (int i = 0; i < 65536; ++i)
    {
      Serial.print("Sending ");
      Serial.print(i);
      Serial.println(".");

      digitalWrite(18, (i & 0b1000000000000000) != 0 ? HIGH : LOW);
      digitalWrite(17, (i & 0b0100000000000000) != 0 ? HIGH : LOW);
      digitalWrite(16, (i & 0b0010000000000000) != 0 ? HIGH : LOW);
      digitalWrite(15, (i & 0b0001000000000000) != 0 ? HIGH : LOW);
      digitalWrite(14, (i & 0b0000100000000000) != 0 ? HIGH : LOW);
      digitalWrite(13, (i & 0b0000010000000000) != 0 ? HIGH : LOW);
      digitalWrite(12, (i & 0b0000001000000000) != 0 ? HIGH : LOW);
      digitalWrite(8,  (i & 0b0000000100000000) != 0 ? HIGH : LOW);
      digitalWrite(7,  (i & 0b0000000010000000) != 0 ? HIGH : LOW);
      digitalWrite(6,  (i & 0b0000000001000000) != 0 ? HIGH : LOW);
      digitalWrite(5,  (i & 0b0000000000100000) != 0 ? HIGH : LOW);
      digitalWrite(4,  (i & 0b0000000000010000) != 0 ? HIGH : LOW);
      digitalWrite(3,  (i & 0b0000000000001000) != 0 ? HIGH : LOW);
      digitalWrite(2,  (i & 0b0000000000000100) != 0 ? HIGH : LOW);
      digitalWrite(1,  (i & 0b0000000000000010) != 0 ? HIGH : LOW);
      digitalWrite(0,  (i & 0b0000000000000001) != 0 ? HIGH : LOW);

      digitalWrite(10, HIGH);
      if (!slowMode)
        delayMicroseconds(25);
      else
        delay(1);
      
      digitalWrite(10, LOW);
      if (!slowMode)
        delayMicroseconds(25);
      else
        delay(1);
    }
    digitalWrite(9, LOW);
    delay(1);

    digitalWrite(22, HIGH);
    digitalWrite(21, LOW);
  }

}
