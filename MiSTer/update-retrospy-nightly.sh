#!/bin/bash

# https://help.github.com/en/github/authenticating-to-github/creating-a-personal-access-token-for-the-command-line
GITHUB_API_TOKEN=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                  
echo "Installing RELEASE_TAG Build of RetroSpy for MiSTer"
echo " "

echo "Updating and executing the RetroSpy installer script."
echo " "

response=$(curl -k -sH "Authorization: token ${GITHUB_API_TOKEN}" https://api.github.com/repos/retrospy/RetroSpy-private/releases/tags/RELEASE_TAG)

eval $(echo "$response" | grep -C3 "name.:.\+update-retrospy-installer.sh" | grep -w id | tr : = | tr -cd '[[:alnum:]]=')
[ "$id" ] || { echo "Error: Failed to get asset id, response: $response" | awk 'length($0)<100' >&2; exit 1; }

GH_ASSET="https://${GITHUB_API_TOKEN}:@api.github.com/repos/retrospy/RetroSpy-private/releases/assets/${id}"

wget -q -t 3 --no-check-certificate --output-file=/tmp/wget-log --show-progress --auth-no-challenge --header "Accept:application/octet-stream" ${GH_ASSET} -O /tmp/update-retrospy-installer.sh

dos2unix /tmp/update-retrospy-installer.sh
chmod +x /tmp/update-retrospy-installer.sh

/tmp/update-retrospy-installer.sh ${GITHUB_API_TOKEN}

rm -rf /tmp/update-retrospy-installer.sh

echo " "
echo "Installation complete! Please go to http://www.retro-spy.com to download the newest Windows client application."
