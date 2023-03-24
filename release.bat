@echo off

IF "%~1"=="" GOTO release

IF NOT "%~1"=="" set sub=1

:release
rmdir /S /Q bin\Release\net7.0
"C:\Program Files\dotnet\dotnet.exe" build RetroSpyX\RetroSpyX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net7.0

if errorlevel 0 goto :AnyCPUBuildgb
echo Aborting release. Error during AnyCPU build.
goto end

:AnyCPUBuildgb
"C:\Program Files\dotnet\dotnet.exe" build GBPemuX\GBPemuX.csproj /p:Configuration=Release /p:Platform="Any CPU"  /p:OutputPath=..\bin\Release\net7.0

if errorlevel 0 goto AnyCPUBuilduu
echo Aborting release. Error during AnyCPU build.
goto end

:AnyCPUBuilduu
"C:\Program Files\dotnet\dotnet.exe" build UsbUpdaterX2\UsbUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net7.0

if errorlevel 0 goto AnyCPUBuildgpbu
echo Aborting release. Error during AnyCPU build.
goto end

:AnyCPUBuildgpbu
"C:\Program Files\dotnet\dotnet.exe" build GBPUpdaterX2\GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net7.0

if errorlevel 0 goto :MiSTer
echo Aborting release. Error during AnyCPU build.
goto end

:MiSTer
cd MiSTer
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\msbuild.exe" MiSTer.vcxproj /p:Configuration=Release /p:Platform="Win32" /p:OutputPath=Release
cd ..

if errorlevel 0 goto GBPNew
echo Aborting release. Error during MiSTer build.
goto end

:GBPNew
if exist "..\..\..\GBP_Firmware\" (
cd firmware
..\sed -i "s/.*\/\/#define.*MODE_GAMEBOY_PRINTER/#define MODE_GAMEBOY_PRINTER/" sketches\firmware.ino
C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328 -build-path D:\pub\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
..\sed -i "s/.*#define.*MODE_GAMEBOY_PRINTER/\/\/#define MODE_GAMEBOY_PRINTER/" sketches\firmware.ino
del ..\..\..\..\GBP_Firmware\firmware.ino.hex
copy D:\pub\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\GBP_Firmware\firmware.ino.hex
cd ..
)

if errorlevel 0 goto GBPOld
echo Aborting release. Error during New Bootloader GBP build.
goto end

:GBPOld
if exist "..\..\..\GBP_Firmware\" (
cd firmware
..\sed -i "s/.*\/\/#define.*MODE_GAMEBOY_PRINTER/#define MODE_GAMEBOY_PRINTER/" sketches\firmware.ino
C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328old -build-path D:\pub\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
..\sed -i "s/.*#define.*MODE_GAMEBOY_PRINTER/\/\/#define MODE_GAMEBOY_PRINTER/" sketches\firmware.ino
del ..\..\..\..\GBP_Firmware\firmware-old.ino.hex
copy D:\pub\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\GBP_Firmware\firmware-old.ino.hex
cd ..
)

if errorlevel 0 goto Vision
echo Aborting release. Error during Old Bootloader GBP build.
goto end

:Vision
if exist "..\..\..\Vision_Firmware\" (
cd firmware
..\sed -i "s/.*\/\/#define.*RS_VISION/#define RS_VISION/" sketches\common.h
C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\arduino-builder.exe -compile -logger=machine -fqbn=arduino:avr:nano:cpu=atmega328old -build-path D:\pub\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release -verbose -hardware "C:/Program Files (x86)/Arduino/hardware" -tools "C:/Program Files (x86)/Arduino/tools-builder" -tools "C:/Program Files (x86)/Arduino/hardware/tools/avr" -built-in-libraries "C:/Program Files (x86)/Arduino/libraries" -libraries C:\Users\Administrator\Documents/Arduino/Libraries -prefs=runtime.tools.ctags.path=C:\Users\Administrator\AppData\Local\VisualGDB\Arduino\tools-builder\ctags\5.8-arduino11 sketches/firmware.ino
..\sed -i "s/.*#define.*RS_VISION/\/\/#define RS_VISION/" sketches\common.h
del ..\..\..\..\Vision_Firmware\firmware.ino.hex
copy D:\pub\src\Repos\retrospy\RetroSpy\firmware\Output\Arduino_Nano\Release\firmware.ino.hex ..\..\..\..\Vision_Firmware\firmware.ino.hex
cd ..
)

if errorlevel 0 goto buildOK
echo Aborting release. Error during Vision Firmware build.
goto end


:buildOK
;del RetroSpy-Windows.zip
;del RetroSpy-Windows.zip.*
;rmdir /S /Q RetroSpy-Setup
;del RetroSpy-Setup.exe
;rmdir /S /Q RetroSpy-Upload

mkdir RetroSpy-Setup

if exist "..\..\..\certs\codesignpasswd.txt" (
    set /p codesignpasswd=<"..\..\..\certs\codesignpasswd.txt"
)

cd "bin\Release\net7.0\"

REM Sign all 4 executables
cd bin\Release\net7.0
if exist "..\..\..\..\..\..\certs\codesign.pfx" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.pfx" /p %codesignpasswd% /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a Retrospy.exe)

if exist "..\..\..\..\..\..\certs\codesign.pfx" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.pfx" /p %codesignpasswd% /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a GBPemu.exe)

if exist "..\..\..\..\..\..\certs\codesign.pfx" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.pfx" /p %codesignpasswd% /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a UsbUpdater.exe)

if exist "..\..\..\..\..\..\certs\codesign.pfx" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.pfx" /p %codesignpasswd% /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a GBPUpdater.exe)

xcopy /y /e /s * ..\..\..\RetroSpy-Setup

cd ..\..\..

cd RetroSpy-Setup
;"C:\Program Files\7-Zip\7z.exe" -r a ..\RetroSpy-Windows.zip *.*
cd ..

;copy LICENSE RetroSpy-Setup
;"C:\Program Files\7-Zip\7z.exe" a RetroSpy-Windows.zip LICENSE

mkdir RetroSpy-Setup\MiSTer
if "%sub%" == "1" ( sed -e s/RELEASE_TAG/%~1/g MiSTer\update-retrospy-nightly.sh > RetroSpy-Setup\MiSTer\update-retrospy.sh) else (copy MiSTer\update-retrospy.sh RetroSpy-Setup\MiSTer)
dos2unix RetroSpy-Setup\MiSTer\update-retrospy.sh
;cd RetroSpy-Setup
;"C:\Program Files\7-Zip\7z.exe" a ..\RetroSpy-Windows.zip MiSTer\update-retrospy.sh
;cd ..
REM ;"C:\Program Files\7-Zip\7z.exe" a RetroSpy-Windows.zip keybindings.xml
REM ;copy keybindings.xml RetroSpy-Setup

REM Copy Drivers
;mkdir RetroSpy-Setup\drivers\
;xcopy /y /e /s drivers RetroSpy-Setup\drivers\
;xcopy /y /e /s CH341SER RetroSpy-Setup\CH341SER\
;copy serial_install.exe RetroSpy-Setup\

if exist "C:\Program Files (x86)\Actual Installer\actinst.exe" (
"C:\Program Files (x86)\Actual Installer\actinst.exe" /S ".\RetroSpy-64.aip"
  if exist "..\..\..\certs\codesign.pfx" (
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\certs\codesign.pfx" /p %codesignpasswd% /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a Retrospy-Setup.exe
  )
)

;mkdir RetroSpy-Upload
copy RetroSpy-Setup.exe RetroSpy-Upload\RetroSpy-Setup-x64.exe
copy RetroSpy-Windows.zip RetroSpy-Upload\RetroSpy-Windows-x64.zip
if "%sub%" == "1" ( copy UsbUpdaterX2\update-usb-retrospy-nightly-installer.sh RetroSpy-Upload\update-usb-retrospy-installer.sh) else (copy UsbUpdaterX2\update-usb-retrospy-installer.sh RetroSpy-Upload)
if "%sub%" == "1" ( sed -e s/RELEASE_TAG/%~1/g MiSTer\update-retrospy-nightly-installer.sh > RetroSpy-Upload\update-retrospy-installer.sh) else (copy MiSTer\update-retrospy-installer.sh RetroSpy-Upload)
;copy MiSTer\Release\retrospy RetroSpy-Upload
if exist "..\..\..\GBP_Firmware\" (
del GBP_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\avrdude.exe
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\avrdude.conf
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\firmware.ino.hex
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\libusb0.dll
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\avrdude
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\libusb-1.0.0.dylib
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\libusb-0.1.4.dylib
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\libftdi1.2.dylib
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\libhidapi.0.dylib
"C:\Program Files\7-Zip\7z.exe" a GBP_Firmware.zip ..\..\..\GBP_Firmware\firmware-old.ino.hex
copy GBP_Firmware.zip RetroSpy-Upload
)
if exist "..\..\..\Vision_Firmware\" (
del Vision_Firmware.zip
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\avrdude.exe
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\avrdude.conf
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\firmware.ino.hex
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\libusb0.dll
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\avrdude
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\libusb-1.0.0.dylib
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\libusb-0.1.4.dylib
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\libftdi1.2.dylib
"C:\Program Files\7-Zip\7z.exe" a Vision_Firmware.zip ..\..\..\Vision_Firmware\libhidapi.0.dylib
copy Vision_Firmware.zip RetroSpy-Upload
)



if exist "..\..\..\kernel\kernel.tar.gz" (
copy ..\..\..\kernel\kernel.tar.gz RetroSpy-Upload
)
if exist "..\..\..\upload\RetroSpy-Linux-x64.tar.gz" (
copy ..\..\..\upload\RetroSpy-Linux-x64.tar.gz RetroSpy-Upload
)
if exist "..\..\..\upload\RetroSpy-Linux-arm64.tar.gz" (
copy ..\..\..\upload\RetroSpy-Linux-arm64.tar.gz RetroSpy-Upload
)
if exist "..\..\..\upload\RetroSpyInstall.dmg" (
copy ..\..\..\upload\RetroSpyInstall.dmg RetroSpy-Upload
)
if exist "..\..\..\upload\RetroSpy-Setup-x86.exe" (
copy ..\..\..\upload\RetroSpy-Setup-x86.exe RetroSpy-Upload
)
if exist "..\..\..\upload\RetroSpy-Windows-x86.zip" (
copy ..\..\..\upload\RetroSpy-Windows-x86.zip RetroSpy-Upload
)
if exist "..\..\..\beaglebone\" (
FOR /F %%I IN ('DIR ..\..\..\beaglebone\*.xz /B /O:-D') DO COPY ..\..\..\beaglebone\%%I RetroSpy-Upload & goto end
)
:end
