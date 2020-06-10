echo "Installing RetroSpy for MiSTer"
echo " "

echo "Updating and executing the RetroSpy installer script."
echo " "

wget -q -t 3 --output-file=/tmp/wget-log --show-progress -O /tmp/update-retrospy-installer.sh https://github.com/retrospy/RetroSpy/raw/master/MiSter/update-retrospy-installer.sh

chmod +x /tmp/update-retrospy-installer.sh

/tmp/update-retrospy-installer.sh

rm /tmp/update-retrospy-installer.sh

echo " " 
echo "Installation complete! Please go to http://www.retro-spy.com to download the newest Windows client application."
