#!/bin/bash

set -e

if [ -n "$1" ]; then
  export sub=1
fi 

rm -rf bin/Release/net8.0
rm -rf bin/Release/RetroSpy-Linux
rm -rf RetroSpy-Linux-arm64.tar.gz

dotnet publish RetroSpyX/RetroSpyX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net8.0/ -r linux-arm64 --self-contained
dotnet publish GBPemuX/GBPemuX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net8.0/ -r linux-arm64 --self-contained
dotnet publish GBPUpdaterX2/GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net8.0/ -r linux-arm64 --self-contained
dotnet publish UsbUpdaterX2/UsbUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net8.0/ -r linux-arm64 --self-contained
      
cd bin/Release      
mkdir RetroSpy-Linux      
mkdir RetroSpy-Linux/bin      
cp -r net8.0/publish/* RetroSpy-Linux/bin       
mv RetroSpy-Linux/bin/skins RetroSpy-Linux        
mv RetroSpy-Linux/bin/keybindings.xml RetroSpy-Linux
mv RetroSpy-Linux/bin/vjoybindings.xml RetroSpy-Linux
mv RetroSpy-Linux/bin/game_palettes.cfg RetroSpy-Linux
mkdir RetroSpy-Linux/MiSTer
         
if [ "$sub" = "1" ]
then
    sed -e s/RELEASE_TAG/$1/g ../../MiSTer/update-retrospy-nightly.sh > RetroSpy-Linux/MiSTer/update-retrospy.sh
else
    cp ../../MiSTer/update-retrospy.sh RetroSpy-Linux/MiSTer
fi
dos2unix RetroSpy-Linux/MiSTer/update-retrospy.sh

cp ../../LICENSE RetroSpy-Linux
mv RetroSpy-Linux/bin/RetroSpy RetroSpy-Linux/bin/retrospy
mv RetroSpy-Linux/bin/GBPemu RetroSpy-Linux/bin/pixelview
mv RetroSpy-Linux/bin/GBPUpdater RetroSpy-Linux/bin/pixelupdate
mv RetroSpy-Linux/bin/UsbUpdater RetroSpy-Linux/bin/visionusbupdate
cp -aR net8.0/firmware RetroSpy-Linux/firmware
tar -zcvf ../../RetroSpy-Linux-arm64.tar.gz RetroSpy-Linux
if [ -d "/mnt/src/upload" ]
then
    cp ../../RetroSpy-Linux-arm64.tar.gz /mnt/src/upload  
fi
cd ../..

exit 0;

