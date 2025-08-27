
IF "%~1"=="" GOTO release

IF NOT "%~1"=="" set sub=1

:release
rmdir /S /Q bin\Release\net8.0
"C:\Program Files\dotnet\dotnet.exe" build RetroSpyX\RetroSpyX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net8.0
if %ERRORLEVEL% NEQ 0 goto :fail

"C:\Program Files\dotnet\dotnet.exe" build GBPemuX\GBPemuX.csproj /p:Configuration=Release /p:Platform="Any CPU"  /p:OutputPath=..\bin\Release\net8.0
if %ERRORLEVEL% NEQ 0 goto :fail

"C:\Program Files\dotnet\dotnet.exe" build UsbUpdaterX2\UsbUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net8.0
if %ERRORLEVEL% NEQ 0 goto :fail

"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" GBPUpdaterX2\GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net8.0
if %ERRORLEVEL% NEQ 0 goto :fail

cd MiSTer
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe" MiSTer.vcxproj /p:Configuration=Release /p:Platform="Win32" /p:OutputPath=Release
if %ERRORLEVEL% NEQ 0 goto :fail
cd ..
if %ERRORLEVEL% NEQ 0 goto :fail

if exist "..\..\..\Firmware\GBP_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*MODE_GAMEBOY_PRINTER/#define MODE_GAMEBOY_PRINTER/" sketches\firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328 -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*MODE_GAMEBOY_PRINTER/\/\/#define MODE_GAMEBOY_PRINTER/" sketches\firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\GBP_Firmware\firmware.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\GBP_Firmware\firmware.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\GBP_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*MODE_GAMEBOY_PRINTER/#define MODE_GAMEBOY_PRINTER/" sketches\firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328old -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*MODE_GAMEBOY_PRINTER/\/\/#define MODE_GAMEBOY_PRINTER/" sketches\firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\GBP_Firmware\firmware-old.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\GBP_Firmware\firmware-old.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Vision_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION$/#define RS_VISION/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328old -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION/\/\/#define RS_VISION/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\Vision_Firmware\firmware-old.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\Vision_Firmware\firmware-old.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Vision_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION$/#define RS_VISION/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328 -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION/\/\/#define RS_VISION/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\Vision_Firmware\firmware.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\Vision_Firmware\firmware.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Dream_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_DREAM/#define RS_VISION_DREAM/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=teensy:avr:teensy40:usb=serial,speed=816,opt=o3std,keys=en-us -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Teensy_4_0\Release -verbose -hardware "C:\Program Files (x86)\Arduino/hardware" -tools "C:\Program Files (x86)\Arduino/tools-builder" -tools "C:\Program Files (x86)\Arduino/hardware/tools/avr" -built-in-libraries "C:\Program Files (x86)\Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_DREAM/\/\/#define RS_VISION_DREAM/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\Dream_Firmware\firmware.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Teensy_4_0\Release\firmware.ino.hex ..\..\..\..\Firmware\Dream_Firmware\firmware.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\CV_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_COLECOVISION/#define RS_VISION_COLECOVISION/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328old -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_COLECOVISION/\/\/#define RS_VISION_COLECOVISION/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\CV_Firmware\firmware-old.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\CV_Firmware\firmware-old.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\CV_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_COLECOVISION/#define RS_VISION_COLECOVISION/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328 -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_COLECOVISION/\/\/#define RS_VISION_COLECOVISION/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\CV_Firmware\firmware.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\CV_Firmware\firmware.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\ADB_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_PIPPIN/#define RS_VISION_PIPPIN/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328old -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_PIPPIN/\/\/#define RS_VISION_PIPPIN/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\ADB_Firmware\firmware-old.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\ADB_Firmware\firmware-old.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\ADB_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_PIPPIN/#define RS_VISION_PIPPIN/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328 -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_PIPPIN/\/\/#define RS_VISION_PIPPIN/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\ADB_Firmware\firmware.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\ADB_Firmware\firmware.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\CDi_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_CDI/#define RS_VISION_CDI/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=rp2040:rp2040:rpipico:flash=2097152_0,freq=250,opt=Optimize3,rtti=Disabled,stackprotect=Disabled,exceptions=Disabled,dbgport=Disabled,dbglvl=None,usbstack=picosdk,ipbtstack=ipv4only,uploadmethod=default -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Raspberry_Pi_Pico\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -hardware C:\Users\Administrator\AppData\Local/Arduino15/packages -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools C:\Users\Administrator\AppData\Local/Arduino15/packages -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.pqt-gcc.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-gcc\4.0.1-8ec9d6f -prefs=runtime.tools.pqt-python3.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-python3\1.0.1-base-3a57aed-1 -prefs=runtime.tools.pqt-elf2uf2.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-elf2uf2\4.0.1-8ec9d6f -prefs=runtime.tools.pqt-openocd.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-openocd\4.0.1-8ec9d6f -prefs=runtime.tools.pqt-picotool.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-picotool\4.0.1-8ec9d6f -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_CDI/\/\/#define RS_VISION_CDI/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\CDi_Firmware\firmware.ino.uf2
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Raspberry_Pi_Pico\Release\firmware.ino.uf2 ..\..\..\..\Firmware\CDi_Firmware\firmware.ino.uf2
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Flex_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_FLEX/#define RS_VISION_FLEX/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=rp2040:rp2040:rpipico:flash=2097152_0,freq=250,opt=Optimize3,rtti=Disabled,stackprotect=Disabled,exceptions=Disabled,dbgport=Disabled,dbglvl=None,usbstack=picosdk,ipbtstack=ipv4only,uploadmethod=default -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Raspberry_Pi_Pico\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -hardware C:\Users\Administrator\AppData\Local/Arduino15/packages -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools C:\Users\Administrator\AppData\Local/Arduino15/packages -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.pqt-gcc.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-gcc\4.0.1-8ec9d6f -prefs=runtime.tools.pqt-python3.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-python3\1.0.1-base-3a57aed-1 -prefs=runtime.tools.pqt-elf2uf2.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-elf2uf2\4.0.1-8ec9d6f -prefs=runtime.tools.pqt-openocd.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-openocd\4.0.1-8ec9d6f -prefs=runtime.tools.pqt-picotool.path=C:\Users\Administrator\AppData\Local\Arduino15\packages\rp2040\tools\pqt-picotool\4.0.1-8ec9d6f -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_FLEX/\/\/#define RS_VISION_FLEX/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\Flex_Firmware\firmware.ino.uf2
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Raspberry_Pi_Pico\Release\firmware.ino.uf2 ..\..\..\..\Firmware\Flex_Firmware\firmware.ino.uf2
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Analog_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_ANALOG_1/#define RS_VISION_ANALOG_1/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328 -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_ANALOG_1/\/\/#define RS_VISION_ANALOG_1/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\Analog_Firmware\firmware_1.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\Analog_Firmware\firmware_1.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  
  ..\sed -i "s/.*\/\/#define.*RS_VISION_ANALOG_2/#define RS_VISION_ANALOG_2/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328 -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_ANALOG_2/\/\/#define RS_VISION_ANALOG_2/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\Analog_Firmware\firmware_2.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\Analog_Firmware\firmware_2.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Analog_Firmware\" (
  cd firmware
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*\/\/#define.*RS_VISION_ANALOG_1/#define RS_VISION_ANALOG_1/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328old -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_ANALOG_1/\/\/#define RS_VISION_ANALOG_1/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\Analog_Firmware\firmware-old.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\Analog_Firmware\firmware_1-old.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  
  ..\sed -i "s/.*\/\/#define.*RS_VISION_ANALOG_2/#define RS_VISION_ANALOG_2/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328old -build-path D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
  if %ERRORLEVEL% NEQ 0 goto :fail
  ..\sed -i "s/.*#define.*RS_VISION_ANALOG_2/\/\/#define RS_VISION_ANALOG_2/" sketches\common.h
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\Analog_Firmware\firmware-old.ino.hex
  copy D:\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Firmware\Analog_Firmware\firmware_2-old.ino.hex
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..
  if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\USBLite_Firmware\" (
  cd ..\usb-sniffer-lite\firmware\make
  if %ERRORLEVEL% NEQ 0 goto :fail
  make
  if %ERRORLEVEL% NEQ 0 goto :fail
  del ..\..\..\..\..\Firmware\USBLite_Firmware\UsbSnifferLite.uf2
  copy D:\src\Repos\retrospy\usb-sniffer-lite\firmware\make\build\UsbSnifferLite.uf2 ..\..\..\..\..\Firmware\USBLite_Firmware\UsbSnifferLite.uf2
  if %ERRORLEVEL% NEQ 0 goto :fail
  cd ..\..\..\RetroSpy
  if %ERRORLEVEL% NEQ 0 goto :fail
)

del RetroSpy-Windows.zip
del RetroSpy-Windows.zip.*
rmdir /S /Q RetroSpy-Setup
del RetroSpy-Setup.exe
rmdir /S /Q RetroSpy-Upload

mkdir RetroSpy-Setup
if %ERRORLEVEL% NEQ 0 goto :fail

if exist "..\..\..\certs\codesignpasswd.txt" ( 
    set /p codesignpasswd=<"..\..\..\certs\codesignpasswd.txt"
)

REM Sign all 4 executables
cd "bin\Release\net8.0\"
if %ERRORLEVEL% NEQ 0 goto :fail

if exist "..\..\..\..\..\..\certs\codesign.cer" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a Retrospy.exe
if %ERRORLEVEL% NEQ 0 goto :fail
)

:signgbpemu
if exist "..\..\..\..\..\..\certs\codesign.cer" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a GBPemu.exe
if %ERRORLEVEL% NEQ 0 goto :fail
)

:signusbupdater
if exist "..\..\..\..\..\..\certs\codesign.cer" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a UsbUpdater.exe
if %ERRORLEVEL% NEQ 0 goto :fail
)

:signgbpupdater
if exist "..\..\..\..\..\..\certs\codesign.cer" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a GBPUpdater.exe
if %ERRORLEVEL% NEQ 0 goto :fail
)

xcopy /y /e /s * ..\..\..\RetroSpy-Setup
if %ERRORLEVEL% NEQ 0 goto :fail

cd ..\..\..
if %ERRORLEVEL% NEQ 0 goto :fail

cd RetroSpy-Setup
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" -r a ..\RetroSpy-Windows.zip *.*
if %ERRORLEVEL% NEQ 0 goto :fail
cd ..
if %ERRORLEVEL% NEQ 0 goto :fail

copy LICENSE RetroSpy-Setup
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a RetroSpy-Windows.zip LICENSE
if %ERRORLEVEL% NEQ 0 goto :fail

mkdir RetroSpy-Setup\MiSTer
if %ERRORLEVEL% NEQ 0 goto :fail
if "%sub%" == "1" ( 
    sed -e s/RELEASE_TAG/%~1/g MiSTer\update-retrospy-nightly.sh > RetroSpy-Setup\MiSTer\update-retrospy.sh
    if %ERRORLEVEL% NEQ 0 goto :fail
) else (
    copy MiSTer\update-retrospy.sh RetroSpy-Setup\MiSTer
)
dos2unix RetroSpy-Setup\MiSTer\update-retrospy.sh
if %ERRORLEVEL% NEQ 0 goto :fail
cd RetroSpy-Setup
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ..\RetroSpy-Windows.zip MiSTer\update-retrospy.sh
if %ERRORLEVEL% NEQ 0 goto :fail
cd ..
if %ERRORLEVEL% NEQ 0 goto :fail
REM ;"C:\Program Files\7-Zip\7z.exe" a RetroSpy-Windows.zip keybindings.xml
REM ;copy keybindings.xml RetroSpy-Setup

REM Copy Drivers
mkdir RetroSpy-Setup\drivers\
if %ERRORLEVEL% NEQ 0 goto :fail
xcopy /y /e /s drivers RetroSpy-Setup\drivers\
if %ERRORLEVEL% NEQ 0 goto :fail
xcopy /y /e /s CH341SER RetroSpy-Setup\CH341SER\
if %ERRORLEVEL% NEQ 0 goto :fail
copy serial_install.exe RetroSpy-Setup\
if %ERRORLEVEL% NEQ 0 goto :fail
copy GBPUpdaterX2\CH340G_2019.zip RetroSpy-Setup\
if %ERRORLEVEL% NEQ 0 goto :fail

if exist "C:\Program Files (x86)\Actual Installer\actinst.exe" (
"C:\Program Files (x86)\Actual Installer\actinst.exe" /S ".\RetroSpy-64.aip"
if %ERRORLEVEL% NEQ 0 goto :fail
  if exist "..\..\..\certs\codesign.cer" (
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x86\SignTool" sign /f "..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a Retrospy-Setup.exe
    if %ERRORLEVEL% NEQ 0 goto :fail
  )
)

mkdir RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
copy RetroSpy-Setup.exe RetroSpy-Upload\RetroSpy-Setup-x64.exe
if %ERRORLEVEL% NEQ 0 goto :fail
copy RetroSpy-Windows.zip RetroSpy-Upload\RetroSpy-Windows-x64.zip
if %ERRORLEVEL% NEQ 0 goto :fail
if "%sub%" == "1" ( 
    copy UsbUpdaterX2\update-usb-retrospy-nightly-installer.sh RetroSpy-Upload\update-usb-retrospy-installer.sh
    if %ERRORLEVEL% NEQ 0 goto :fail
) else (
    copy UsbUpdaterX2\update-usb-retrospy-installer.sh RetroSpy-Upload
    if %ERRORLEVEL% NEQ 0 goto :fail
)
if "%sub%" == "1" ( 
    sed -e s/RELEASE_TAG/%~1/g MiSTer\update-retrospy-nightly-installer.sh > RetroSpy-Upload\update-retrospy-installer.sh
    if %ERRORLEVEL% NEQ 0 goto :fail
) else (
    copy MiSTer\update-retrospy-installer.sh RetroSpy-Upload
    if %ERRORLEVEL% NEQ 0 goto :fail
)
copy MiSTer\Release\retrospy RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
if exist "..\..\..\Firmware\GBP_Firmware\" (
del GBP_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\avrdude.exe
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\avrdude.conf
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\firmware.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\libusb0.dll
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\avrdude
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\libusb-1.0.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\libusb-0.1.4.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\libftdi1.2.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\libhidapi.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\Firmware\GBP_Firmware\firmware-old.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
copy GBP_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Vision_Firmware\" (
del Vision_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\avrdude.exe
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\avrdude.conf
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\firmware.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\firmware-old.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\libusb0.dll
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\avrdude
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\libusb-1.0.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\libusb-0.1.4.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\libftdi1.2.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Firmware\Vision_Firmware\libhidapi.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
copy Vision_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Dream_Firmware\" (
del Dream_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a Dream_Firmware.zip ..\..\..\Firmware\Dream_Firmware\firmware.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Dream_Firmware.zip ..\..\..\Firmware\Dream_Firmware\teensy_loader_cli.linux
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Dream_Firmware.zip ..\..\..\Firmware\Dream_Firmware\teensy_loader_cli.exe
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Dream_Firmware.zip ..\..\..\Firmware\Dream_Firmware\teensy_loader_cli.mac
if %ERRORLEVEL% NEQ 0 goto :fail
copy Dream_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\CV_Firmware\" (
del CV_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\avrdude.exe
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\avrdude.conf
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\firmware.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\firmware-old.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\libusb0.dll
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\avrdude
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\libusb-1.0.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\libusb-0.1.4.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\libftdi1.2.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a CV_Firmware.zip ..\..\..\Firmware\CV_Firmware\libhidapi.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
copy CV_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\ADB_Firmware\" (
del ADB_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\avrdude.exe
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\avrdude.conf
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\firmware.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\firmware-old.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\libusb0.dll
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\avrdude
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\libusb-1.0.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\libusb-0.1.4.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\libftdi1.2.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a ADB_Firmware.zip ..\..\..\Firmware\ADB_Firmware\libhidapi.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
copy ADB_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\CDi_Firmware\" (
del CDi_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a CDi_Firmware.zip ..\..\..\Firmware\CDi_Firmware\firmware.ino.uf2
if %ERRORLEVEL% NEQ 0 goto :fail
copy CDi_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\USBLite_Firmware\" (
del USBLite_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a USBLite_Firmware.zip ..\..\..\Firmware\USBLite_Firmware\UsbSnifferLite.uf2
if %ERRORLEVEL% NEQ 0 goto :fail
copy USBLite_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\Firmware\Flex_Firmware\" (
del Flex_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a Flex_Firmware.zip ..\..\..\Firmware\Flex_Firmware\firmware.ino.uf2
if %ERRORLEVEL% NEQ 0 goto :fail
copy Flex_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)


if exist "..\..\..\Firmware\Analog_Firmware\" (
del Analog_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\avrdude.exe
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\avrdude.conf
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\firmware_1.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\firmware_2.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\firmware_1-old.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\firmware_2-old.ino.hex
if %ERRORLEVEL% NEQ 0 goto :fail+
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\libusb0.dll
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\avrdude
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\libusb-1.0.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\libusb-0.1.4.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\libftdi1.2.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\7-Zip\7z.exe" a Analog_Firmware.zip ..\..\..\Firmware\Analog_Firmware\libhidapi.0.dylib
if %ERRORLEVEL% NEQ 0 goto :fail
copy Analog_Firmware.zip RetroSpy-Upload
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\kernel\kernel.tar.gz" (
  copy ..\..\..\kernel\kernel.tar.gz RetroSpy-Upload
  if %ERRORLEVEL% NEQ 0 goto :fail
)
if exist "..\..\..\upload\RetroSpy-Linux-x64.tar.gz" (
  copy ..\..\..\upload\RetroSpy-Linux-x64.tar.gz RetroSpy-Upload
  if %ERRORLEVEL% NEQ 0 goto :fail
)
if exist "..\..\..\upload\RetroSpy-Linux-arm64.tar.gz" (
  copy ..\..\..\upload\RetroSpy-Linux-arm64.tar.gz RetroSpy-Upload
  if %ERRORLEVEL% NEQ 0 goto :fail
)
if exist "..\..\..\upload\RetroSpyInstall.dmg" (
  copy ..\..\..\upload\RetroSpyInstall.dmg RetroSpy-Upload
  if %ERRORLEVEL% NEQ 0 goto :fail
)
if exist "..\..\..\upload\RetroSpy-Setup-x86.exe" (
  copy ..\..\..\upload\RetroSpy-Setup-x86.exe RetroSpy-Upload
  if %ERRORLEVEL% NEQ 0 goto :fail
)
if exist "..\..\..\upload\RetroSpy-Windows-x86.zip" (
  copy ..\..\..\upload\RetroSpy-Windows-x86.zip RetroSpy-Upload
  if %ERRORLEVEL% NEQ 0 goto :fail
)
if exist "..\..\..\beaglebone\" (
  FOR /F %%I IN ('DIR ..\..\..\beaglebone\*.xz /B /O:-D') DO (
    COPY ..\..\..\beaglebone\%%I RetroSpy-Upload
    if %ERRORLEVEL% NEQ 0 goto :fail
    goto :end
  )
)

:end
EXIT /b 0

:fail
EXIT /b 1