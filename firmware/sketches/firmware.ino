///////////////////////////////////////////////////////////////////////////////
// RetroSpy Firmware for Arduino Uno & Teensy 3.5/4.0/4.1
// Version: 6.1.4
// RetroSpy written by zoggins of RetroSpy Technologies
// NintendoSpy originally written by jaburns

// NOTE: If you are having input display lag problems try uncommenting out this line.
// #define LAG_FIX
// WARNING!!! If you do uncomment out this line you must enable "Options -> Use Lag Fix" in the UI

// ---------- Uncomment one of these options to select operation mode ---------
// 
//-- Arduino or Teensy 3.5
//#define MODE_NES
//#define MODE_POWERGLOVE
//#define MODE_SNES
//#define MODE_N64
//#define MODE_GC
//#define MODE_SMS
//#define MODE_SMS_PADDLE
//#define MODE_SMS_SPORTS_PAD
//#define MODE_GENESIS
//#define MODE_SMS_ON_GENESIS // For using a Genesis cable and the Genesis 
							  // reader in the exe while playing SMS games.
//#define MODE_GENESIS_MOUSE
//#define MODE_SATURN
//#define MODE_SATURN3D
//#define MODE_PLAYSTATION
//#define MODE_GBA

//-- Arduino Only
//#define MODE_BOOSTER_GRIP
//#define MODE_TG16
//#define MODE_NEOGEO
//#define MODE_3DO
//#define MODE_INTELLIVISION
//#define MODE_JAGUAR
//#define MODE_FMTOWNS
//#define MODE_PCFX
//#define MODE_AMIGA_KEYBOARD
//#define MODE_AMIGA_MOUSE
//#define MODE_CDI_KEYBOARD
//#define MODE_GAMEBOY_PRINTER
//#define MODE_VFLASH

//--- Teensy 3.5 Only
//#define MODE_DREAMCAST
//#define MODE_WII
//#define MODE_CD32
//#define MODE_FMTOWNS_KEYBOARD_AND_MOUSE
//#define MODE_VSMILE

//--- Teensy 4.0 Only
//#define MODE_NUON

//Bridge GND to the right analog IN to enable your selected mode
//#define MODE_DETECT

//--- Require Arduino + 3rd Party Libraries.  Setup is more complicated
//#define MODE_CDI
//#define MODE_CDTV_WIRED
//#define MODE_CDTV_WIRELESS
//#define MODE_COLECOVISION
//#define MODE_DRIVING_CONTROLLER
//#define MODE_PIPPIN
//#define MODE_KEYBOARD_CONTROLLER
//#define MODE_KEYBOARD_CONTROLLER_STAR_RAIDERS
//#define MODE_KEYBOARD_CONTROLLER_BIG_BIRD

//--- Require 2 Arduinos.  Setup is A LOT more complicated.
//#define MODE_AMIGA_ANALOG_1
//#define MODE_AMIGA_ANALOG_2
//#define MODE_ATARI5200_1
//#define MODE_ATARI5200_2
//#define MODE_COLECOVISION_ROLLER
//#define MODE_ATARI_PADDLES

// Some consoles care about PAL/NTSC for timing purposes
#define VIDEO_OUTPUT VIDEO_PAL

// CD-i controller timeouts (ms)
#define CDI_WIRED_TIMEOUT 50
#define CDI_WIRELESS_TIMEOUT 100

// Pippin Controller Configuration
#define PIPPIN_CONTROLLER_SPY_ADDRESS 0xF
#define PIPPIN_MOUSE_SPY_ADDRESS 0xE

///////////////////////////////////////////////////////////////////////////////
// ---------- NOTHING BELOW THIS LINE SHOULD BE MODIFIED  -------------------//
///////////////////////////////////////////////////////////////////////////////

#include "common.h"

#include "NES.h"
#include "SNES.h"
#include "N64.h"
#include "GC.h"
#include "GBA.h"

#include "BoosterGrip.h"
#include "Genesis.h"
#include "GenesisMouse.h"
#include "SMS.h"
#include "SMSPaddle.h"
#include "SMSSportsPad.h"
#include "Saturn.h"
#include "Saturn3D.h"

#include "ColecoVision.h"
#include "FMTowns.h"
#include "Intellivision.h"
#include "Jaguar.h"
#include "NeoGeo.h"
#include "PCFX.h"
#include "PlayStation.h"
#include "TG16.h"
#include "ThreeDO.h"
#include "CDiKeyboard.h"
#include "GameBoyPrinterEmulator.h"
#include "CDi.h"

#include "Dreamcast.h"
#include "AmigaCd32.h"
#include "Wii.h"
#include "FMTownsKeyboardAndMouse.h"

#include "DrivingController.h"
#include "Pippin.h"
#include "AmigaKeyboard.h"
#include "AmigaMouse.h"
#include "CDTVWired.h"
#include "CDTVWireless.h"
#include "AmigaAnalog.h"
#include "Atari5200.h"
#include "KeyboardController.h"
#include "ColecoVisionRoller.h"
#include "AtariPaddles.h"
#include "PowerGlove.h"
#include "Nuon.h"
#include "VSmile.h"
#include "VFlash.h"

bool CreateSpy();

ControllerSpy* currentSpy = NULL;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// General initialization, just sets all pins to input and starts serial communication.
void setup()
{

	// FOR MODE DETECTION
#if defined(RS_VISION_ULTRA)
	for (int i = 13; i <= 18; ++i)
		pinMode(i, INPUT_PULLUP);
#elif defined(__arm__) && defined(CORE_TEENSY)
	for (int i = 33; i < 40; ++i)
		pinMode(i, INPUT_PULLUP);
#elif defined(ARDUINO_AVR_NANO_EVERY)
	for (int i = 3; i < 9; ++i)
		if (i != 7)
			pinMode(i, INPUT_PULLUP);
#elif defined(RASPBERRYPI_PICO) || defined(ARDUINO_RASPBERRY_PI_PICO)
	pinMode(MODEPIN_SNES, INPUT_PULLUP);
	pinMode(MODEPIN_WII, INPUT_PULLUP);
#elif defined(RS_VISION) || defined(RS_VISION_CDI)
	for (int i = A0; i <= A7; ++i)
		pinMode(i, INPUT_PULLUP);
#elif !defined(MODE_ATARI_PADDLES) && !defined(MODE_ATARI5200_1) && !defined(MODE_ATARI5200_2) && !defined(MODE_AMIGA_ANALOG_1) && !defined(MODE_AMIGA_ANALOG_2)
	PORTC = 0xFF; // Set the pull-ups on the port we use to check operation mode.
	DDRC  = 0x00;
#endif

#ifdef LAG_FIX
	Serial.begin(57600);
#else
	Serial.begin(115200);
#endif

	if (!CreateSpy() && currentSpy != NULL)	
	{
		currentSpy->setup();
	}

	if (currentSpy != NULL)
	{
		currentSpy->printFirmwareInfo();
	}

	#pragma GCC diagnostic push
	#pragma GCC diagnostic ignored "-Wunused-value"
	T_DELAY(5000);
	A_DELAY(200);
	#pragma GCC diagnostic pop
 
}

#if defined(RASPBERRYPI_PICO)  || defined(ARDUINO_RASPBERRY_PI_PICO)
void setup1()
{
	ControllerSpy* volatile *p = &currentSpy;
	while (*p == NULL)
	{
	}
	currentSpy->setup1();
}
#endif

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Arduino sketch main loop definition.
void loop()
{
	if (currentSpy != NULL)
		currentSpy->loop();
}

#if defined(RASPBERRYPI_PICO)  || defined(ARDUINO_RASPBERRY_PI_PICO)
void loop1()
{
	if (currentSpy != NULL)
		currentSpy->loop1();
}
#endif

#if defined(RS_VISION) || defined(RS_VISION_CDI)
byte ReadAnalog()
{
	byte retVal = PINC;
	
	return (~retVal  & 0b00111111);
}
#endif

#if defined(RS_VISION_ULTRA)
byte ReadAnalog4()
{
	Serial.println("here");
	byte retVal = 0x00;
	
	for (int i = 0; i < 6; ++i)
	{
		if (digitalReadFast(i + 13) == LOW)
			retVal |= (1 << i);
	}
	Serial.println(retVal);
	return retVal;
}
#endif

bool CreateSpy()
{
	bool customSetup = false;
#if defined(RS_VISION)
	switch (ReadAnalog())
	{
	case 0x00:
		currentSpy = new NESSpy();
		break;
	case 0x01:
		currentSpy = new PowerGloveSpy();
		break;
	case 0x02:
		currentSpy = new SNESSpy();
		break;
	case 0x03:
		currentSpy = new N64Spy();
		break;
	case 0x04:
		currentSpy = new GCSpy();
		break;
	case 0x05:
		currentSpy = new SMSSpy();		
		((SMSSpy*)currentSpy)->setup(SMSSpy::CABLE_GENESIS);
		customSetup = true;
		break;	
	case 0x06:
		currentSpy = new SMSPaddleSpy();
		((SMSPaddleSpy*)currentSpy)->setup(SMSPaddleSpy::CABLE_GENESIS);
		customSetup = true;
		break;
	case 0x07:
		currentSpy = new SMSSportsPadSpy();
		break;
	case 0x08:
		currentSpy = new GenesisSpy();
		break;
	case 0x09:
		currentSpy = new GenesisMouseSpy();
		break;
	case 0x0A:
		currentSpy = new SaturnSpy();
		break;
	case 0x0B:
		currentSpy = new Saturn3DSpy();
		break;
	case 0x0C:
		currentSpy = new PlayStationSpy();
		break;
	case 0x0D:
		currentSpy = new GBASpy();
		break;
	case 0x0E:
		currentSpy = new BoosterGripSpy();
		((BoosterGripSpy*)currentSpy)->setup(BoosterGripSpy::CABLE_GENESIS);
		customSetup = true;
		break;
	case 0x0F:
		currentSpy = new TG16Spy();
		break;
	case 0x10:
		currentSpy = new NeoGeoSpy();
		break;
	case 0x11:
		currentSpy = new ThreeDOSpy();
		((ThreeDOSpy*)currentSpy)->setup(ThreeDOSpy::CABLE_GENESIS);
		customSetup = true;
		break;
	case 0x12:
		currentSpy = new IntellivisionSpy();
		break;	
	case 0x13:
		currentSpy = new JaguarSpy();
		break;
	case 0x14:
		currentSpy = new FMTownsSpy();
		break;
	case 0x15:
		currentSpy = new PCFXSpy();
		break;
	case 0x16:
		currentSpy = new AmigaKeyboardSpy();
		break;
	case 0x17:
		currentSpy = new AmigaMouseSpy();
		((AmigaMouseSpy*)currentSpy)->setup(VIDEO_PAL, AmigaMouseSpy::CABLE_GENESIS);
		customSetup = true;
		break;
	case 0x18:
		currentSpy = new AmigaMouseSpy();
		((AmigaMouseSpy*)currentSpy)->setup(VIDEO_NTSC, AmigaMouseSpy::CABLE_GENESIS);
		customSetup = true;
		break;
	case 0x19:
		currentSpy = new CDTVWiredSpy();
		break;
	case 0x1A:
		currentSpy = new ColecoVisionSpy();
		break;
//	case 0x1B:
//		currentSpy = new PippinSpy();
//		((PippinSpy*)currentSpy)->setup(PIPPIN_CONTROLLER_SPY_ADDRESS, PIPPIN_MOUSE_SPY_ADDRESS);
//		customSetup = true;
//		break;
	case 0x1C:
		currentSpy = new KeyboardControllerSpy();
		((KeyboardControllerSpy*)currentSpy)->setup(KeyboardControllerSpy::MODE_NORMAL, KeyboardControllerSpy::CABLE_GENESIS);
		customSetup = true;
		break;
	case 0x1D:
		currentSpy = new KeyboardControllerSpy();
		((KeyboardControllerSpy*)currentSpy)->setup(KeyboardControllerSpy::MODE_STAR_RAIDERS, KeyboardControllerSpy::CABLE_GENESIS);
		customSetup = true;
		break;
	case 0x1E:
		currentSpy = new KeyboardControllerSpy();
		((KeyboardControllerSpy*)currentSpy)->setup(KeyboardControllerSpy::MODE_BIG_BIRD, KeyboardControllerSpy::CABLE_GENESIS);
		customSetup = true;
		break;
	case 0x1F:
		currentSpy = new DrivingControllerSpy();
		((DrivingControllerSpy*)currentSpy)->setup(DrivingControllerSpy::CABLE_GENESIS);
		customSetup = true;
		break;		
	case 0x20:
		currentSpy = new SMSSpy();		
		((SMSSpy*)currentSpy)->setup(SMSSpy::CABLE_GX4000);
		customSetup = true;
		break;	
	}
#elif defined(RS_VISION_ULTRA)
	switch (ReadAnalog4())
	{
	case 0x00:
		currentSpy = new DreamcastSpy();
		break;
	case 0x01:
		currentSpy = new N64Spy();
		break;
	case 0x02:
		currentSpy = new GCSpy();
		break;	
	case 0x03:
		currentSpy = new WiiSpy();
		break;
	case 0x04:
		currentSpy = new NuonSpy();
		break;
	}
#elif defined(RS_VISION_CDI)
	switch (ReadAnalog())
	{
	case 0x00:
		currentSpy = new CDiSpy(CDI_WIRED_TIMEOUT, CDI_WIRELESS_TIMEOUT);
		break;
	case 0x01:
		currentSpy = new CDiKeyboardSpy();
		break;
	case 0x02:
		currentSpy = new CDTVWirelessSpy();
		break;
	}
#elif defined(RS_VISION_COLECOVISION)
	switch (ReadAnalog())
	{
	case 0x00:
		currentSpy = new ColecoVisionSpy();
		break;
	case 0x01:
		currentSpy = new ColecoVisionRollerSpy();
		((ColecoVisionRollerSpy*)currentSpy)->setup(VIDEO_NTSC);
		customSetup = true;
		break;	
	case 0x02:
		currentSpy = new ColecoVisionRollerSpy();
		((ColecoVisionRollerSpy*)currentSpy)->setup(VIDEO_PAL);
		customSetup = true;
		break;	
	}
#elif defined(MODE_DETECT)
	if (!PINC_READ(MODEPIN_SNES))
		currentSpy = new SNESSpy;
#if !defined(RASPBERRYPI_PICO)
	else if (!PINC_READ(MODEPIN_N64))
		currentSpy = new N64Spy();
	else if (!PINC_READ(MODEPIN_GC))
		currentSpy = new GCSpy();
#endif
#if defined(__arm__) && defined(CORE_TEENSY)
	else if (!PINC_READ(MODEPIN_DREAMCAST))
		currentSpy = new DreamcastSpy();
#endif
#if (defined(__arm__) && defined(CORE_TEENSY)) || defined(RASPBERRYPI_PICO)  || defined(ARDUINO_RASPBERRY_PI_PICO)
	else if (!PINC_READ(MODEPIN_WII))
		currentSpy = new WiiSpy();
#endif 
#if !defined(RASPBERRYPI_PICO) && !defined(ARDUINO_RASPBERRY_PI_PICO)
	else
		currentSpy = new NESSpy();
#endif
#elif defined(MODE_NES)
	currentSpy = new NESSpy();
#elif defined(MODE_POWERGLOVE)
	currentSpy = new PowerGloveSpy();
#elif defined(MODE_SNES)
	currentSpy = new SNESSpy();
#elif defined(MODE_N64)
	currentSpy = new N64Spy();
#elif defined(MODE_GC)
	currentSpy = new GCSpy();
#elif defined(MODE_GBA)
	currentSpy = new GBASpy();
#elif defined(MODE_BOOSTER_GRIP)
	currentSpy = new BoosterGripSpy();
#elif defined(MODE_GENESIS)
	currentSpy = new GenesisSpy();
#elif defined(MODE_GENESIS_MOUSE)
	currentSpy = new GenesisMouseSpy();
#elif defined(MODE_SMS)
	currentSpy = new SMSSpy();
#elif defined(MODE_SMS_PADDLE)
	currentSpy = new SMSPaddleSpy();
#elif defined(MODE_SMS_SPORTS_PAD)
	currentSpy = new SMSSportsPadSpy();
#elif defined(MODE_SMS_ON_GENESIS)
	currentSpy = new SMSSpy();
	((SMSSpy*)currentSpy)->setup(SMSSpy::CABLE_GENESIS, SMSSpy::OUTPUT_GENESIS);
	customSetup = true;
#elif defined(MODE_SATURN)
	currentSpy = new SaturnSpy();
#elif defined(MODE_SATURN3D)
	currentSpy = new Saturn3DSpy();
#elif defined(MODE_COLECOVISION)
	currentSpy = new ColecoVisionSpy();
#elif defined(MODE_FMTOWNS)
	currentSpy = new FMTownsSpy();
#elif defined(MODE_INTELLIVISION)
	currentSpy = new IntelliVisionSpy();
#elif defined(MODE_JAGUAR)
	currentSpy = new JaguarSpy();
#elif defined(MODE_NEOGEO)
	currentSpy = new NeoGeoSpy();
# elif defined(MODE_PCFX)
	currentSpy = new PCFXSpy();
#elif  defined(MODE_PLAYSTATION)
	currentSpy = new PlayStationSpy();
#elif defined(MODE_TG16)
	currentSpy = new TG16Spy();
#elif defined(MODE_3DO)
	currentSpy = new ThreeDOSpy();
#elif defined(MODE_DREAMCAST)
	currentSpy = new DCSpy();
#elif defined(MODE_WII)
	currentSpy = new WiiSpy();
#elif defined(MODE_CD32)
	currentSpy = new Cd32Spy();
#elif defined(MODE_DRIVING_CONTROLLER)
	currentSpy = new DrivingControllerSpy();
#elif defined(MODE_PIPPIN)
	currentSpy = new PippinSpy();
	((PippinSpy*)currentSpy)->setup(PIPPIN_CONTROLLER_SPY_ADDRESS, PIPPIN_MOUSE_SPY_ADDRESS);
	customSetup = true;
#elif defined(MODE_AMIGA_KEYBOARD)
	currentSpy = new AmigaKeyboardSpy();
#elif defined(MODE_AMIGA_MOUSE)                                            
	currentSpy = new AmigaMouseSpy();
	((AmigaMouseSpy*)currentSpy)->setup(VIDEO_OUTPUT);
	customSetup = true;
#elif defined(MODE_CDTV_WIRED)
	currentSpy = new CDTVWiredSpy();
#elif defined(MODE_CDTV_WIRELESS)
	currentSpy = new CDTVWirelessSpy();
#elif defined(MODE_FMTOWNS_KEYBOARD_AND_MOUSE)
	currentSpy = new FMTownsKeyboardAndMouseSpy();
#elif defined(MODE_CDI)
	currentSpy = new CDiSpy(CDI_WIRED_TIMEOUT, CDI_WIRELESS_TIMEOUT);
#elif defined(MODE_CDI_KEYBOARD)
	currentSpy = new CDiKeyboardSpy();
#elif defined(MODE_GAMEBOY_PRINTER)
	currentSpy = new GameBoyPrinterEmulator();
#elif defined(MODE_AMIGA_ANALOG_1)
	currentSpy = new AmigaAnalogSpy();
	((AmigaAnalogSpy*)currentSpy)->setup(false);
	customSetup = true;
#elif defined(MODE_AMIGA_ANALOG_2)
	currentSpy = new AmigaAnalogSpy();
	((AmigaAnalogSpy*)currentSpy)->setup(true);
	customSetup = true;
#elif defined(MODE_ATARI5200_1) 
	currentSpy = new Atari5200Spy();
	((Atari5200Spy*)currentSpy)->setup(false);
	customSetup = true;
#elif defined(MODE_ATARI5200_2)
	currentSpy = new Atari5200Spy();
	((Atari5200Spy*)currentSpy)->setup(true);
	customSetup = true;
#elif defined(MODE_COLECOVISION_ROLLER)                                                  
	currentSpy = new ColecoVisionRollerSpy();
	((ColecoVisionRollerSpy*)currentSpy)->setup(VIDEO_OUTPUT);
	customSetup = true;
#elif defined(MODE_ATARI_PADDLES)                                                  
	currentSpy = new AtariPaddlesSpy();
#elif defined(MODE_NUON)                                                  
	currentSpy = new NuonSpy();
#elif defined(MODE_VSMILE)                                                  
	currentSpy = new VSmileSpy();
#elif defined(MODE_VFLASH)                                                  
	currentSpy = new VFlashSpy();
#elif defined(MODE_KEYBOARD_CONTROLLER) 
	currentSpy = new KeyboardControllerSpy();
	((KeyboardControllerSpy*)currentSpy)->setup(KeyboardControllerSpy::MODE_NORMAL);
	customSetup = true;
#elif (MODE_KEYBOARD_CONTROLLER_STAR_RAIDERS) 
	currentSpy = new KeyboardControllerSpy();
	((KeyboardControllerSpy*)currentSpy)->setup(KeyboardControllerSpy::MODE_STAR_RAIDERS);
	customSetup = true;
#elif (MODE_KEYBOARD_CONTROLLER_BIG_BIRD)
	currentSpy = new KeyboardControllerSpy();
	((KeyboardControllerSpy*)currentSpy)->setup(KeyboardControllerSpy::MODE_BIG_BIRD);
	customSetup = true;
#endif
	
	return customSetup;
}
