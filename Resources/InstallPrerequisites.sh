#/bin/sh
echo Updating APT repositories
sudo apt-get update -q -y
sudo apt-get upgrade -q -y

echo Installing prerequisites for running photobooth
sudo apt-get install joe mono-complete git --install-suggests -q -y

echo Installing prerequisites for building photobooth
sudo apt-get install mono-xbuild gtk-sharp2 libgtk-sharp-beans*--install-suggests -q -y


