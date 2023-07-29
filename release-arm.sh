#!/bin/bash

if [ -n "$1" ]; then
  export sub=1
fi 

rm -rf bin/Release/net7.0
rm -rf bin/Release/RetroSpy-Linux
rm -rf RetroSpy-Linux-arm64.tar.gz

dotnet publish RetroSpyX/RetroSpyX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ -r linux-arm64 --self-contained

if [ $? -ne 0 ] 
then 
   echo "Aborting release. Error during RetroSpyX build."
else
   dotnet publish GBPemuX/GBPemuX.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ -r linux-arm64 --self-contained
   if [ $? -ne 0 ] 
   then 
     echo "Aborting release. Error during GBPemuX build."
   else
     dotnet publish GBPUpdaterX2/GBPUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ -r linux-arm64 --self-contained
     if [ $? -ne 0 ] 
     then 
       echo "Aborting release. Error during GBPUpdater build."
     else
       dotnet publish UsbUpdaterX2/UsbUpdaterX2.csproj /p:Configuration=Release /p:Platform="Any CPU" /p:OutputPath=../bin/Release/net7.0/ -r linux-arm64 --self-contained
       if [ $? -ne 0 ] 
       then 
         echo "Aborting release. Error during GBPUpdater build."
       else
         cd bin/Release
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
	     mkdir RetroSpy-Linux
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
         mkdir RetroSpy-Linux/bin
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
         cp -r net7.0/publish/* RetroSpy-Linux/bin
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
         mv RetroSpy-Linux/bin/skins RetroSpy-Linux
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
         mv RetroSpy-Linux/bin/keybindings.xml RetroSpy-Linux
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
         mv RetroSpy-Linux/bin/game_palettes.cfg RetroSpy-Linux
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
         mkdir RetroSpy-Linux/MiSTer
         if [ $? -ne 0 ]
         then
           exit 1;
         fi

         if [ "$sub" = "1" ]
         then
           sed -e s/RELEASE_TAG/$1/g ../../MiSTer/update-retrospy-nightly.sh > RetroSpy-Linux/MiSTer/update-retrospy.sh
           if [ $? -ne 0 ]
           then
             exit 1;
           fi
         else
           cp ../../MiSTer/update-retrospy.sh RetroSpy-Linux/MiSTer
           if [ $? -ne 0 ]
           then
             exit 1;
           fi
         fi
         dos2unix RetroSpy-Linux/MiSTer/update-retrospy.sh
         if [ $? -ne 0 ]
         then
           exit 1;
         fi

         cp ../../LICENSE RetroSpy-Linux
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
	     mv RetroSpy-Linux/bin/RetroSpy RetroSpy-Linux/bin/retrospy
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
	     mv RetroSpy-Linux/bin/GBPemu RetroSpy-Linux/bin/pixelview
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
	     mv RetroSpy-Linux/bin/GBPUpdater RetroSpy-Linux/bin/pixelupdate
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
	     mv RetroSpy-Linux/bin/UsbUpdater RetroSpy-Linux/bin/visionusbupdate
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
         cp -aR net7.0/firmware RetroSpy-Linux/firmware
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
	     tar -zcvf ../../RetroSpy-Linux-arm64.tar.gz RetroSpy-Linux
         if [ $? -ne 0 ]
         then
           exit 1;
         fi
         if [ -d "/mnt/src/upload" ]
         then
           cp ../../RetroSpy-Linux-arm64.tar.gz /mnt/src/upload  
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
	   fi
     fi
   fi
fi

exit 1;

