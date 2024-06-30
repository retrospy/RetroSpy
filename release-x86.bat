
IF "%~1"=="" GOTO release

IF NOT "%~1"=="" set sub=1

:release
rmdir /S /Q bin\Release\net8.0
"C:\Program Files\dotnet\dotnet.exe" build RetroSpyX\RetroSpyX.csproj /p:RuntimeIdentifier=win-x86 /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net8.0
if %ERRORLEVEL% NEQ 0 goto :fail

"C:\Program Files\dotnet\dotnet.exe" build GBPemuX\GBPemuX.csproj /p:RuntimeIdentifier=win-x86 /p:Configuration=Release /p:Platform="Any CPU"  /p:OutputPath=..\bin\Release\net8.0
if %ERRORLEVEL% NEQ 0 goto :fail

"C:\Program Files\dotnet\dotnet.exe" build UsbUpdaterX2\UsbUpdaterX2.csproj /p:RuntimeIdentifier=win-x86 /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net8.0
if %ERRORLEVEL% NEQ 0 goto :fail

"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\amd64\MSBuild.exe" GBPUpdaterX2\GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net8.0
if %ERRORLEVEL% NEQ 0 goto :fail

cd MiSTer
if %ERRORLEVEL% NEQ 0 goto :fail
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\msbuild.exe" MiSTer.vcxproj /p:Configuration=Release /p:Platform="Win32" /p:OutputPath=Release
if %ERRORLEVEL% NEQ 0 goto :fail
cd ..
if %ERRORLEVEL% NEQ 0 goto :fail

del RetroSpy-Windows.zip
del RetroSpy-Windows.zip.*
rmdir /S /Q RetroSpy-Setup
del RetroSpy-Setup.exe
rmdir /S /Q RetroSpy-Upload

mkdir RetroSpy-Setup
if %ERRORLEVEL% NEQ 0 goto :fail

if exist "..\..\..\certs\codesignpasswd.txt" (
    set /p codesignpasswd=<"..\..\..\certs\codesignpasswd.txt"
    if %ERRORLEVEL% NEQ 0 goto :fail
)

REM Sign all 4 executables
cd "bin\Release\net8.0\"
if %ERRORLEVEL% NEQ 0 goto :fail

if exist "..\..\..\..\..\..\certs\codesign.cer" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a Retrospy.exe
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\..\..\..\certs\codesign.cer" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a GBPemu.exe
if %ERRORLEVEL% NEQ 0 goto :fail)

if exist "..\..\..\..\..\..\certs\codesign.cer" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a UsbUpdater.exe
if %ERRORLEVEL% NEQ 0 goto :fail
)

if exist "..\..\..\..\..\..\certs\codesign.cer" (
"C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a GBPUpdater.exe
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
    if %ERRORLEVEL% NEQ 0 goto :fail
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
"C:\Program Files (x86)\Actual Installer\actinst.exe" /S ".\RetroSpy-32.aip"
if %ERRORLEVEL% NEQ 0 goto :fail
  if exist "..\..\..\certs\codesign.cer" (
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\certs\codesign.cer" /csp "eToken Base Cryptographic Provider" /k "%codesignpasswd%" /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a Retrospy-Setup.exe
    if %ERRORLEVEL% NEQ 0 goto :fail
  )
)

if exist "..\..\..\upload\" (
  copy RetroSpy-Setup.exe ..\..\..\upload\RetroSpy-Setup-x86.exe
  if %ERRORLEVEL% NEQ 0 goto :fail
  copy RetroSpy-Windows.zip ..\..\..\upload\RetroSpy-Windows-x86.zip
  if %ERRORLEVEL% NEQ 0 goto :fail
)

:end
EXIT /b 0

:fail
EXIT /b 1