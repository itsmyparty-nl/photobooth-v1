#!/bin/bash

echo "## Executing configure_xorg.sh..."
PHOTOBOOTH_USER="photobooth"

sudo apt update
sudo apt install -y i3 xorg xinit

# Edit Bashrc to Start X automatically

STARTX='if [ -z "$DISPLAY" ] && [ -n "$XDG_VTNR" ] && [ "$XDG_VTNR" -eq 1 ]; then
    # Disable screen blanking / power saving
    xset s off
    xset -dpms
    xset s noblank
    # Start Xorg
    exec startx
fi
'

touch /home/$PHOTOBOOTH_USER/.bashrc
printf "%s\n\n" "$STARTX" | cat - /home/$PHOTOBOOTH_USER/.bashrc > /home/$PHOTOBOOTH_USER/.bashrc.new && cp /home/$PHOTOBOOTH_USER/.bashrc.new /home/$PHOTOBOOTH_USER/.bashrc

# Ensure you login automatically
GETTY_FILE="/usr/lib/systemd/system/getty@.service"
sudo -S sed -i 's|^ExecStart=-/sbin/agetty -o .*|ExecStart=-/sbin/agetty --autologin '"$PHOTOBOOTH_USER"' --noclear %I $TERM|' "$GETTY_FILE"

# Copy i3 config file
mkdir -p /home/$PHOTOBOOTH_USER/.config/i3
cp ./i3.config /home/$PHOTOBOOTH_USER/.config/i3/config
sudo chown -R $PHOTOBOOTH_USER:$PHOTOBOOTH_USER /home/$PHOTOBOOTH_USER/.config
sudo chmod +x /home/$PHOTOBOOTH_USER/.config/i3/config

