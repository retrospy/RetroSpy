#!/bin/bash

if [ -n "$1" ]; then
  export sub=1
fi   

rm -rf bin/Release/RetroSpy-macOS
rm -rf RetroSpy-macOS.zip

mkdir bin/Release/RetroSpy-macOS
if [ $? -ne 0 ]
then
  exit 1;
fi

rm -rf bin/Release/net7.0
/usr/local/share/dotnet/dotnet publish RetroSpyX/RetroSpyX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-x64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/RetroSpy bin/Release/net7.0/publish/RetroSpy-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
/usr/local/share/dotnet/dotnet publish RetroSpyX/RetroSpyX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-arm64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/RetroSpy bin/Release/net7.0/publish/RetroSpy-arm64
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64
if [ $? -ne 0 ]
then
  exit 1;
fi
lipo -create -output bin/Release/net7.0/publish/RetroSpy bin/Release/net7.0/publish/RetroSpy-arm64 bin/Release/net7.0/publish/RetroSpy-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
lipo -create -output bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64 bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
rm bin/Release/net7.0/publish/*-arm64
rm bin/Release/net7.0/publish/*-x64

mkdir bin/Release/RetroSpy-macOS/RetroSpy.app
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir bin/Release/RetroSpy-macOS/RetroSpy.app/Contents
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/MacOS
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/Resources
if [ $? -ne 0 ]
then
  exit 1;
fi
cp RetroSpyX/Info.plist bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/
if [ $? -ne 0 ]
then
  exit 1;
fi
cp RetroSpyX/RetroSpy.icns bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/Resources/
if [ $? -ne 0 ]
then
  exit 1;
fi
cp -aR bin/Release/net7.0/publish/* bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/MacOS
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/MacOS/skins bin/Release/RetroSpy-macOS/
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/RetroSpy-macOS/RetroSpy.app/Contents/MacOS/keybindings.xml bin/Release/RetroSpy-macOS/
if [ $? -ne 0 ]
then
  exit 1;
fi
cp -aR bin/Release/net7.0/firmware bin/Release/RetroSpy-macOS/firmware
if [ $? -ne 0 ]
then
  exit 1;
fi

mkdir bin/Release/RetroSpy-macOS/MiSTer
if [ $? -ne 0 ]
then
  exit 1;
fi
if [ "$sub" = "1" ]
then
    sed -e s/RELEASE_TAG/$1/g MiSTer/update-retrospy-nightly.sh > bin/Release/RetroSpy-macOS/MiSTer/update-retrospy.sh
    if [ $? -ne 0 ]
    then
      exit 1;
    fi
else
    cp MiSTer/update-retrospy.sh bin/Release/RetroSpy-macOS/MiSTer
    if [ $? -ne 0 ]
    then
      exit 1;
    fi
fi
dos2unix bin/Release/RetroSpy-macOS/MiSTer/update-retrospy.sh
if [ $? -ne 0 ]
then
  exit 1;
fi

cp LICENSE bin/Release/RetroSpy-macOS
if [ $? -ne 0 ]
then
  exit 1;
fi

rm -rf bin/Release/net7.0
/usr/local/share/dotnet/dotnet publish GBPemuX/GBPemuX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-x64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/GBPemu bin/Release/net7.0/publish/GBPemu-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
/usr/local/share/dotnet/dotnet publish GBPemuX/GBPemuX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-arm64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/GBPemu bin/Release/net7.0/publish/GBPemu-arm64
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64
if [ $? -ne 0 ]
then
  exit 1;
fi
lipo -create -output bin/Release/net7.0/publish/GBPemu bin/Release/net7.0/publish/GBPemu-arm64 bin/Release/net7.0/publish/GBPemu-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
lipo -create -output bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64 bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
rm bin/Release/net7.0/publish/*-arm64
rm bin/Release/net7.0/publish/*-x64

mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/MacOS"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/Resources"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp GBPemuX/Info.plist "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp GBPemuX/GBPemu.icns "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/Resources/"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp -aR bin/Release/net7.0/publish/* "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/MacOS"
if [ $? -ne 0 ]
then
  exit 1;
fi
mv "bin/Release/RetroSpy-macOS/RetroSpy Pixel Viewer.app/Contents/MacOS/game_palettes.cfg" bin/Release/RetroSpy-macOS/
if [ $? -ne 0 ]
then
  exit 1;
fi

rm -rf bin/Release/net7.0
/usr/local/share/dotnet/dotnet publish GBPUpdaterX2/GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-x64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/GBPUpdater bin/Release/net7.0/publish/GBPUpdater-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
/usr/local/share/dotnet/dotnet publish GBPUpdaterX2/GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-arm64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/GBPUpdater bin/Release/net7.0/publish/GBPUpdater-arm64
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64
if [ $? -ne 0 ]
then
  exit 1;
fi
lipo -create -output bin/Release/net7.0/publish/GBPUpdater bin/Release/net7.0/publish/GBPUpdater-arm64 bin/Release/net7.0/publish/GBPUpdater-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
lipo -create -output bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-arm64 bin/Release/net7.0/publish/libSystem.IO.Ports.Native.dylib-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
rm bin/Release/net7.0/publish/*-arm64
rm bin/Release/net7.0/publish/*-x64

mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/MacOS"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/Resources"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp GBPUpdaterX2/Info.plist "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp GBPUpdaterX2/GBPUpdater.icns "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/Resources/"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp -aR bin/Release/net7.0/publish/* "bin/Release/RetroSpy-macOS/RetroSpy Pixel Updater.app/Contents/MacOS"
if [ $? -ne 0 ]
then
  exit 1;
fi

rm -rf bin/Release/net7.0
/usr/local/share/dotnet/dotnet publish UsbUpdaterX2/UsbUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-x64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/UsbUpdater bin/Release/net7.0/publish/UsbUpdater-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
/usr/local/share/dotnet/dotnet publish UsbUpdaterX2/UsbUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ /p:RuntimeIdentifier=osx-arm64 /p:SelfContained=true -p:PublishSingleFile=true -p:UseAppHost=true
if [ $? -ne 0 ]
then
  exit 1;
fi
mv bin/Release/net7.0/publish/UsbUpdater bin/Release/net7.0/publish/UsbUpdater-arm64
if [ $? -ne 0 ]
then
  exit 1;
fi
lipo -create -output bin/Release/net7.0/publish/UsbUpdater bin/Release/net7.0/publish/UsbUpdater-arm64 bin/Release/net7.0/publish/UsbUpdater-x64
if [ $? -ne 0 ]
then
  exit 1;
fi
rm bin/Release/net7.0/publish/*-arm64
rm bin/Release/net7.0/publish/*-x64

mkdir "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/MacOS"
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/Resources"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp UsbUpdaterX2/Info.plist "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp UsbUpdaterX2/UsbUpdater.icns "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/Resources/"
if [ $? -ne 0 ]
then
  exit 1;
fi
cp -aR bin/Release/net7.0/publish/* "bin/Release/RetroSpy-macOS/RetroSpy Vision USB Updater.app/Contents/MacOS"
if [ $? -ne 0 ]
then
  exit 1;
fi

cd bin/Release/RetroSpy-macOS/
if [ $? -ne 0 ]
then
  exit 1;
fi

security unlock-keychain -p "$keychain_password" /Users/zoggins/Library/Keychains/login.keychain
if [ $? -ne 0 ]
then
  exit 1;
fi

find "RetroSpy.app/Contents/MacOS/"|while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --deep --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "$fname"
    if [ $? -ne 0 ]
    then
      exit 1;
    fi
  fi
done
echo "[INFO] Signing app file"
codesign --deep --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist RetroSpy.app
if [ $? -ne 0 ]
then
  exit 1;
fi

find "RetroSpy Pixel Viewer.app/Contents/MacOS/"|while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --deep --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "$fname"
    if [ $? -ne 0 ]
    then
      exit 1;
    fi
  fi
done
echo "[INFO] Signing app file"
codesign --deep --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "RetroSpy Pixel Viewer.app"
if [ $? -ne 0 ]
then
  exit 1;
fi

find "RetroSpy Pixel Updater.app/Contents/MacOS/"|while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --deep --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "$fname"
    if [ $? -ne 0 ]
    then
      exit 1;
    fi
  fi
done
echo "[INFO] Signing app file"
codesign --deep --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "RetroSpy Pixel Updater.app"
if [ $? -ne 0 ]
then
  exit 1;
fi

find "RetroSpy Vision USB Updater.app/Contents/MacOS/"|while read fname; do
  if [[ -f $fname ]]; then
    echo "[INFO] Signing $fname"
    codesign --deep --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "$fname"
    if [ $? -ne 0 ]
    then
      exit 1;
    fi
  fi
done
echo "[INFO] Signing app file"
codesign --deep --force --verbose --timestamp --sign "$apple_teamid" --options=runtime --entitlements ../../../entitlements.plist "RetroSpy Vision USB Updater.app"
if [ $? -ne 0 ]
then
  exit 1;
fi

cd ..
if [ $? -ne 0 ]
then
  exit 1;
fi
ditto -c --sequesterRsrc -k RetroSpy-macOS/ ../../RetroSpy-macOS.zip
if [ $? -ne 0 ]
then
  exit 1;
fi
xcrun notarytool submit ../../RetroSpy-macOS.zip --wait --apple-id "$apple_username" --password "$apple_password" --team-id "$apple_teamid" --output-format json
if [ $? -ne 0 ]
then
  exit 1;
fi

xcrun stapler staple "RetroSpy-macOS/RetroSpy.app"
if [ $? -ne 0 ]
then
  exit 1;
fi
xcrun stapler staple "RetroSpy-macOS/RetroSpy Pixel Viewer.app"
if [ $? -ne 0 ]
then
  exit 1;
fi
xcrun stapler staple "RetroSpy-macOS/RetroSpy Pixel Updater.app"
if [ $? -ne 0 ]
then
  exit 1;
fi
xcrun stapler staple "RetroSpy-macOS/RetroSpy Vision USB Updater.app"
if [ $? -ne 0 ]
then
  exit 1;
fi

rm ../../RetroSpy-macOS.zip
#ditto -c --sequesterRsrc -k RetroSpy-macOS/ ../../RetroSpy-macOS.zip

rm -rf RetroSpyInstall
rm -rf ../../RetroSpyInstall.dmg
mkdir RetroSpyInstall
if [ $? -ne 0 ]
then
  exit 1;
fi
mkdir RetroSpyInstall/RetroSpy
if [ $? -ne 0 ]
then
  exit 1;
fi
fileicon set RetroSpyInstall/RetroSpy ../../Folder.icns
if [ $? -ne 0 ]
then
  exit 1;
fi
cp -aR RetroSpy-macOS/* RetroSpyInstall/RetroSpy/
if [ $? -ne 0 ]
then
  exit 1;
fi

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
    if [ $? -ne 0 ]
    then
      exit 1;
    fi
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
    if [ $? -ne 0 ]
    then
      exit 1;
    fi
fi

#  hdiutil create /tmp/tmp.dmg -ov -volname "RetroSpyInstall" -fs HFS+ -srcfolder "RetroSpyInstall"
#  hdiutil convert /tmp/tmp.dmg -format UDZO -o ../../RetroSpyInstall.dmg 

rm -rf RetroSpyInstall

codesign --deep --force --verbose --timestamp --sign "$apple_teamid" ../../RetroSpyInstall.dmg
if [ $? -ne 0 ]
then
  exit 1;
fi

xcrun notarytool submit ../../RetroSpyInstall.dmg --wait --apple-id "$apple_username" --password "$apple_password" --team-id "$apple_teamid" --output-format json
if [ $? -ne 0 ]
then
  exit 1;
fi
xcrun stapler staple ../../RetroSpyInstall.dmg
if [ $? -ne 0 ]
then
  exit 1;
fi

if [ -d "/Volumes/src/upload" ]
then
  cp ../../RetroSpyInstall.dmg /Volumes/src/upload  
  if [ $? -ne 0 ]
  then
    exit 1;
  fi
fi
cd ../..
if [ $? -ne 0 ]
then
  exit 1;
fi

exit 0;
