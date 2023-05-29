#!/bin/bash

[ $# -ne 1 ] && { echo "Usage: $0 [GitHub Personal Access Token]"; exit 1; }
 
GITHUB_API_TOKEN=$1

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
echo "Installing RELEASE_TAG Build of RetroSpy for MiSTer Server..."
echo ""

#Sync Files
response=$(curl -k -sH "Authorization: token ${GITHUB_API_TOKEN}" https://api.github.com/repos/retrospy/RetroSpy-private/releases/tags/RELEASE_TAG)

eval $(echo "$response" | grep -C3 "name.:.\+retrospy\"" | grep -w id | tr : = | tr -cd '[[:alnum:]]=')
[ "$id" ] || { echo "Error: Failed to get asset id, response: $response" | awk 'length($0)<100' >&2; exit 1; }

RETROSPY_URL="https://${GITHUB_API_TOKEN}:@api.github.com/repos/retrospy/RetroSpy-private/releases/assets/${id}"

wget -P "${RETROSPY_PATH}" -q -nc -t 3 --no-check-certificate --show-progress --auth-no-challenge --header "Accept:application/octet-stream" ${RETROSPY_URL}
mv ${RETROSPY_PATH}/${id} ${RETROSPY_PATH}/retrospy
