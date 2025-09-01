#/bin/sh
echo Paste the following in /etc/udev/rules.d/80-serialnames.rules
echo KERNEL=="ttyAMA0",SYMLINK+="ttyS0" GROUP="dialout"
echo KERNEL=="ttyACM0",SYMLINK+="ttyS1" GROUP="dialout"
cp ./80-serialnames.rules /etc/udev/rules.d/
echo Add Pi user to dialout group
sudo addgroup pi dialout