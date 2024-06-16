#!/bin/sh

echo Starting Photobooth in 10 seconds
sleep 10

cd /home/patrick/photobooth
export PATH=$PATH:/home/patrick/dotnet
export DOTNET_ROOT=/home/patrick/dotnet
./PhotoBoothGUI
