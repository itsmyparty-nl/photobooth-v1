#/bin/sh
#Source: http://www.drewkeller.com/blog/adding-hardware-clock-raspberry-pi-ds3231

# Comment out the blacklist entry so the module can be loaded on boot 
sudo sed -i 's/blacklist i2c-bcm2708/#blacklist i2c-bcm2708/' /etc/modprobe.d/raspi-blacklist.conf
# Load the module now
sudo modprobe i2c-bcm2708
# Notify Linux of the Dallas RTC device (use -0 for Model A or -1 for Model B)
echo ds3231 0x68 | sudo tee /sys/class/i2c-adapter/i2c-1/new_device
# Test whether Linux can see our RTC module.
sudo hwclock

#Disable other clock suppliers
#sudo update-rc.d ntp disable
sudo update-rc.d fake-hwclock disable

#Copy adapted hwclock init script to init folder
sudo cp ~/projects/photobooth/resources/hwclock2 /etc/init.d/
sudo update-rc.d hwclock disable
sudo update-rc.d hwclock2 enable

#Update the time from ntp & write to the hwclock
sudo ntpd -gq
sudo hwclock -w
 
 