#!/bin/bash

echo "## Executing install_photobooth.sh..."

sudo apt update
sudo apt install -y plymouth-theme-spinner

sudo cp ./ItsMyPartyBg.png /usr/share/plymouth/themes/spinner/splash.png
sudo chown root:root /usr/share/plymouth/themes/spinner/splash.png
sudo chmod 644 /usr/share/plymouth/themes/spinner/splash.png

sudo cp ./ItsMyPartyBg.png /usr/share/plymouth/themes/spinner/watermark.png
sudo chown root:root /usr/share/plymouth/themes/spinner/watermark.png
sudo chmod 644 /usr/share/plymouth/themes/spinner/watermark.png

sudo plymouth-set-default-theme -R spinner
sudo update-grub
