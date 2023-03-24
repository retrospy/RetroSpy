#!/bin/bash

if [ -n "$1" ]; then
  export sub=1
fi   

rm -rf bin/Release/RetroSpy-macOS
rm -rf RetroSpy-macOS.zip

mkdir bin/Release/RetroSpy-macOS

rm -rf bin/Release/net7.0
/usr/local/share/dotnet/dotnet publish RetroSpyX/RetroSpyX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-x64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
mv bin/Release/net7.0/publish/RetroSpy bin/Release/net7.0/publish/RetroSpy-x64
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
/usr/local/share/dotnet/dotnet publish RetroSpyX/RetroSpyX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-arm64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
mv bin/Release/net7.0/publish/RetroSpy bin/Release/net7.0/publish/RetroSpy-arm64
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64
lipo -create -output bin/Release/net7.0/publish/RetroSpy bin/Release/net7.0/publish/RetroSpy-arm64 bin/Release/net7.0/publish/RetroSpy-x64
lipo -create -output bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64 bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
rm bin/Release/net7.0/publish/*-arm64
rm bin/Release/net7.0/publish/*-x64

mkdir bin/Release/RetroSpy-macOS/RetroSpy.app
mkdir bin/Release/RetroSpy-macOS/RetroSpy.app/Contents
mkdir bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/MacOS
mkdir bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/Resources
cp RetroSpyX/Info.plist bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/
cp RetroSpyX/RetroSpy.icns bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/Resources/
cp -aR bin/Release/net7.0/publish/* bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/MacOS
mv bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/MacOS/skins bin/Release/RetroSpy-macOS/
mv bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/MacOS/keybindings.xml bin/Release/RetroSpy-macOS/
cp -aR bin/Release/net7.0/firmware bin/Release/RetroSpy-macOS/firmware

mkdir bin/Release/RetroSpy-macOS/MiSTer
if [ "$sub" = "1" ]
then
    sed -e s/RELEASE_TAG/$1/g MiSTer/update-retrospy-nightly.sh > bin/Release/RetroSpy-macOS/MiSTer/update-retrospy.sh
else
    cp MiSTer/update-retrospy.sh bin/Release/RetroSpy-macOS/MiSTer
fi
dos2unix bin/Release/RetroSpy-macOS/MiSTer/update-retrospy.sh

cp LICENSE bin/Release/RetroSpy-macOS
 
rm -rf bin/Release/net7.0
/usr/local/share/dotnet/dotnet publish GBPemuX/GBPemuX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-x64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
mv bin/Release/net7.0/publish/GBPemu bin/Release/net7.0/publish/GBPemu-x64
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
/usr/local/share/dotnet/dotnet publish GBPemuX/GBPemuX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-arm64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
mv bin/Release/net7.0/publish/GBPemu bin/Release/net7.0/publish/GBPemu-arm64
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64
lipo -create -output bin/Release/net7.0/publish/GBPemu bin/Release/net7.0/publish/GBPemu-arm64 bin/Release/net7.0/publish/GBPemu-x64
lipo -create -output bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64 bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
rm bin/Release/net7.0/publish/*-arm64
rm bin/Release/net7.0/publish/*-x64

mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/MacOS"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/Resources"
cp GBPemuX/Info.plist "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/"
cp GBPemuX/GBPemu.icns "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/Resources/"
cp -aR bin/Release/net7.0/publish/* "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/MacOS"
mv "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/MacOS/game_palettes.cfg" bin/Release/RetroSpy-macOS/

rm -rf bin/Release/net7.0
/usr/local/share/dotnet/dotnet publish GBPUpdaterX2/GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-x64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
mv bin/Release/net7.0/publish/GBPUpdater bin/Release/net7.0/publish/GBPUpdater-x64
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
/usr/local/share/dotnet/dotnet publish GBPUpdaterX2/GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-arm64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
mv bin/Release/net7.0/publish/GBPUpdater bin/Release/net7.0/publish/GBPUpdater-arm64
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64
lipo -create -output bin/Release/net7.0/publish/GBPUpdater bin/Release/net7.0/publish/GBPUpdater-arm64 bin/Release/net7.0/publish/GBPUpdater-x64
lipo -create -output bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64 bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
rm bin/Release/net7.0/publish/*-arm64
rm bin/Release/net7.0/publish/*-x64

mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/MacOS"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/Resources"
cp GBPUpdaterX2/Info.plist "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/"
cp GBPUpdaterX2/GBPUpdater.icns "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/Resources/"
cp -aR bin/Release/net7.0/publish/* "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/MacOS"

rm -rf bin/Release/net7.0
/usr/local/share/dotnet/dotnet publish UsbUpdaterX2/UsbUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-x64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
mv bin/Release/net7.0/publish/UsbUpdater bin/Release/net7.0/publish/UsbUpdater-x64
/usr/local/share/dotnet/dotnet publish UsbUpdaterX2/UsbUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-arm64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
mv bin/Release/net7.0/publish/UsbUpdater bin/Release/net7.0/publish/UsbUpdater-arm64
lipo -create -output bin/Release/net7.0/publish/UsbUpdater bin/Release/net7.0/publish/UsbUpdater-arm64 bin/Release/net7.0/publish/UsbUpdater-x64
rm bin/Release/net7.0/publish/*-arm64
rm bin/Release/net7.0/publish/*-x64

mkdir "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/MacOS"
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/Resources"
cp UsbUpdaterX2/Info.plist "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/"
cp UsbUpdaterX2/UsbUpdater.icns "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/Resources/"
cp -aR bin/Release/net7.0/publish/* "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/MacOS"

cd bin/Release/RetroSpy-macOS/

security unlock-keychain -p "$keychain_password" /Users/zoggins/Library/Keychains/login.keychain

find "RetroSpy.app/Contents/MacOS/"|while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "$fname"
  fi
done
echo "[INFO] Signing app file"
codesign --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist RetroSpy.app

find "RetroSpy Pixel Viewer.app/Contents/MacOS/"|while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "$fname"
  fi
done
echo "[INFO] Signing app file"
codesign --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "RetroSpy Pixel Viewer.app"

find "RetroSpy Pixel Updater.app/Contents/MacOS/"|while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "$fname"
  fi
done
echo "[INFO] Signing app file"
codesign --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "RetroSpy Pixel Updater.app"

find "RetroSpy Vision USB Updater.app/Contents/MacOS/"|while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "$fname"
  fi
done
echo "[INFO] Signing app file"
codesign --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "RetroSpy Vision USB Updater.app"

cd ..
ditto -c --sequesterRsrc -k RetroSpy-macOS/ ../../RetroSpy-macOS.zip
xcrun notarytool submit ../../RetroSpy-macOS.zip --wait --apple-id "$apple_username" --password "$apple_password" --team-id "$apple_teamid" --output-format json

xcrun stapler staple "RetroSpy-macOS/RetroSpy.app"
xcrun stapler staple "RetroSpy-macOS/RetroSpy Pixel Viewer.app"
xcrun stapler staple "RetroSpy-macOS/RetroSpy Pixel Updater.app"
xcrun stapler staple "RetroSpy-macOS/RetroSpy Vision USB Updater.app"

rm ../../RetroSpy-macOS.zip
#ditto -c --sequesterRsrc -k RetroSpy-macOS/ ../../RetroSpy-macOS.zip

rm -rf RetroSpyInstall
rm -rf ../../RetroSpyInstall.dmg
mkdir RetroSpyInstall
mkdir RetroSpyInstall/RetroSpy
fileicon set RetroSpyInstall/RetroSpy ../../Folder.icns
cp -aR RetroSpy-macOS/* RetroSpyInstall/RetroSpy/

if [[ -z "${SSH_CLIENT}" ]] && [[ -z "${LAUNCHDRUN}" ]]; 
then
  create-dmg \
    --volname "RetroSpy Installer" \
    --volicon "../../dmgicon.icns" \
    --background "../../installer_background.png" \
    --window-pos 200 120 \
    --window-size 800 400 \
    --icon-size 100 \
    --icon "RetroSpy" 200 190 \
    --app-drop-link 600 185 \
    "../../RetroSpyInstall.dmg" \
    "RetroSpyInstall"
else
  cp -a ../../dmgdstore RetroSpyInstall/.DS_Store
  mkdir RetroSpyInstall/.background
  cp -a ../../installer_background.png RetroSpyInstall/.background
  create-dmg \
    --volname "RetroSpy Installer" \
    --volicon "../../dmgicon.icns" \
    --app-drop-link 600 185 \
    --skip-jenkins \
    "../../RetroSpyInstall.dmg" \
    "RetroSpyInstall"
fi

#  hdiutil create /tmp/tmp.dmg -ov -volname "RetroSpyInstall" -fs HFS+ -srcfolder "RetroSpyInstall"
#  hdiutil convert /tmp/tmp.dmg -format UDZO -o ../../RetroSpyInstall.dmg 

rm -rf RetroSpyInstall

codesign --force --verbose --timestamp --sign "$apple_teamid" ../../RetroSpyInstall.dmg

xcrun notarytool submit ../../RetroSpyInstall.dmg --wait --apple-id "$apple_username" --password "$apple_password" --team-id "$apple_teamid" --output-format json
xcrun stapler staple ../../RetroSpyInstall.dmg


if [ -d "/Volumes/src/upload" ]
then
  cp ../../RetroSpyInstall.dmg /Volumes/src/upload  
fi
cd ../..

