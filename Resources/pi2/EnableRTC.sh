#/bin/sh
#Source: http://www.drewkeller.com/blog/adding-hardware-clock-raspberry-pi-ds3231

# Comment out the blacklist entry so the module can be loaded on boot 
sudo sed -i 's/blacklist i2c-bcm2708/#blacklist i2c-bcm2708/' /etc/modprobe.d/raspi-blacklist.conf

#Disable other clock suppliers
#sudo update-rc.d ntp disable
sudo update-rc.d fake-hwclock disable

#Copy adapted hwclock init script to init folder
sudo cp ~/projects/photobooth/Resources/hwclock2.sh /etc/init.d/
sudo update-rc.d hwclock disable
sudo rm /etc/init.d/hwclock.sh
sudo update-rc.d hwclock2 defaults

#Update the time from ntp & write to the hwclock
sudo ntpd -gq
sudo /etc/init.d/hwclock2 restart 