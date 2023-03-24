@echo off

IF "%~1"=="" GOTO release

IF NOT "%~1"=="" set sub=1

:release
rmdir /S /Q bin\Release\net7.0
"C:\Program Files\dotnet\dotnet.exe" build RetroSpyX\RetroSpyX.csproj /p:RuntimeIdentifier=win10-x86 /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net7.0

if errorlevel 0 goto :AnyCPUBuildgb
echo Aborting release. Error during AnyCPU build.
goto end

:AnyCPUBuildgb
"C:\Program Files\dotnet\dotnet.exe" build GBPemuX\GBPemuX.csproj /p:RuntimeIdentifier=win10-x86 /p:Configuration=Release /p:Platform="Any CPU"  /p:OutputPath=..\bin\Release\net7.0

if errorlevel 0 goto AnyCPUBuilduu
echo Aborting release. Error during AnyCPU build.
goto end

:AnyCPUBuilduu
"C:\Program Files\dotnet\dotnet.exe" build UsbUpdaterX2\UsbUpdaterX2.csproj /p:RuntimeIdentifier=win10-x86 /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net7.0

if errorlevel 0 goto AnyCPUBuildgpbu
echo Aborting release. Error during AnyCPU build.
goto end

:AnyCPUBuildgpbu
"C:\Program Files\dotnet\dotnet.exe" build GBPUpdaterX2\GBPUpdaterX2.csproj /p:RuntimeIdentifier=win10-x86 /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=..\bin\Release\net7.0

if errorlevel 0 goto :MiSTer
echo Aborting release. Error during AnyCPU build.
goto end

:MiSTer
cd MiSTer
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\msbuild.exe" MiSTer.vcxproj /p:Configuration=Release /p:Platform="Win32" /p:OutputPath=Release
cd ..

if errorlevel 0 goto buildOK
echo Aborting release. Error during MiSTer build.
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
"C:\Program Files (x86)\Actual Installer\actinst.exe" /S ".\RetroSpy-32.aip"
  if exist "..\..\..\certs\codesign.pfx" (
    "C:\Program Files (x86)\Windows Kits\10\bin\10.0.22000.0\x86\SignTool" sign /f "..\..\..\certs\codesign.pfx" /p %codesignpasswd% /tr http://timestamp.comodoca.com  /td sha256 /fd sha256 /a Retrospy-Setup.exe
  )
)

if exist "..\..\..\upload\" (
  copy RetroSpy-Setup.exe ..\..\..\upload\RetroSpy-Setup-x86.exe
  copy RetroSpy-Windows.zip ..\..\..\upload\RetroSpy-Windows-x86.zip
)

:end
