#!/bin/bash

echo "## Executing install_photobooth.sh..."

sudo add-apt-repository ppa:dotnet/backports
sudo apt update
sudo apt install -y git gphoto2 libgphoto2-dev dotnet-sdk-9.0

mkdir ~/workspace
cd ~/workspace

git clone https://github.com/itsmyparty-nl/photobooth-v1.git
dotnet publish ./photobooth-v1/src/PhotoBoothGUI/PhotoBoothGUI.csproj -o ~/photobooth

echo "## Photobooth installed to ~/photobooth"
echo "## You can run it with the command: ~/photobooth/PhotoBoothGUI"
echo "## Make sure to update the appsettings.json file in the ~/photobooth folder to your needs."
