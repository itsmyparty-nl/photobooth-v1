#/bin/sh
echo install package for logo support
sudo apt-get install fbi
echo copy splashscreen logo to startup location
sudo cp ItsMyPartyBg.png /etc/splash.png
echo copy init script to startup location
sudo cp asplashscreen /etc/init.d/
echo Make init scrip executable
sudo chmod a+x /etc/init.d/asplashscreen
echo Install script for init mode
sudo insserv /etc/init.d/asplashscreen
