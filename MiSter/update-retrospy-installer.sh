#Github URL
RETROSPY_URL=https://github.com/retrospy/RetroSpy/raw/master/MiSter/retrospy

#Base directory for all script tasks
BASE_PATH="/media/fat"

#RetroSpy Directory
RETROSPY_FOLDER="retrospy"

#RetroSpy Path
RETROSPY_PATH="${BASE_PATH}/${RETROSPY_FOLDER}"

#Make Directories if needed
mkdir -p "${RETROSPY_PATH}"

#RetroSpy file downloading
echo ""
echo "Downloading RetroSpy for MiSTer Server..."
echo ""

#Sync Files
wget -q -nc -t 3 --no-check-certificate --show-progress "${RETROSPY_URL}" -P "${RETROSPY_PATH}"

echo "Complete! Please go to http://www.retro-spy.com to download the newest Windows client application."
