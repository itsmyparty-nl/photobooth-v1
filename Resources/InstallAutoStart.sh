#/bin/sh
echo creating autostart script folder
mkdir ~/.config/autostart
echo Installing photobooth autostart script
cp photobooth.desktop ~/.config/autostart
echo Copying autostart shell script to user folder
cp StartPhotoBooth.sh ~/