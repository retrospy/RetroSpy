#include "common.h"
#include "NeoGeo.h"

inline void read_NeoGeo()
{
  rawData[0] = PIN_READ(NEOGEO_SELECT);
  rawData[1] = PIN_READ(NEOGEO_D);
  rawData[2] = PIN_READ(NEOGEO_B);
  rawData[3] = PIN_READ(NEOGEO_RIGHT);
  rawData[4] = PIN_READ(NEOGEO_DOWN);
  rawData[5] = PIN_READ(NEOGEO_START);
  rawData[6] = PINB_READ(NEOGEO_C);
  rawData[7] = PINB_READ(NEOGEO_A);
  rawData[8] = PINB_READ(NEOGEO_LEFT);
  rawData[9] = PINB_READ(NEOGEO_UP);
}

inline void sendNeoGeoData()
{
    #ifndef DEBUG
    for (unsigned char i = 0; i < 10; ++i)
    {
      Serial.write( rawData[i] ? ZERO : ONE );
    }
    Serial.write( SPLIT );
    #else
    for(int i = 0; i < 10; ++i)
    {
      Serial.print(rawData[i] ? "0" : "1");
    }
    Serial.print("\n");
    #endif
}

inline void loop_NeoGeo()
{
  noInterrupts();
  read_NeoGeo();
  interrupts();
  sendNeoGeoData();
}

