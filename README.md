### Photobooth Repo ###

* A photobooth created using the UDOO development board.
* Version 0.1
* Be sure to check out the development branch for a working version. Master still needs to be merged!

### Description: ###
I created this for my wedding and we had tons of fun with it (both building it, and taking pictures during the wedding).
The photobooth is fully based on open source components, and would not have been possible without the open source software & hardware community.
The full source code is my thank you to the community. Hope you have as much fun with it as we did!

The project ended up as winner of the Udoo Call for Projects: http://www.udoo.org/the-king-of-udooers-wholl-fly-to-rome/

Apart from being a fun project, I also used the project to get acquainted with some new .Net 4.5 features and with Mono on linux, so that might answer
the question on WHY .NET :)

Note: When using this project for your personal photobooth. Be prepared to put in some quality time since setting it up is still quite an effort.
I will spend some effort in making everything more user-friendly, but since I have a busy day time job and a family it might take a while until it is foolproof. 

Note: A few libraries (CmdMessenger & Libgphoto2) are currently integrated in the project as source code. This needs to be cleaned up and changes and additions still need to be submitted to their respectful owners. Just so you know that I'm not trying to take credit for your code.

### Big thank you ###
* To the Arduino people: Without Arduino I never would have used micro controllers! Thanks for starting the open-source hardware madness!
* To Udoo: Combining a powerful ARM board with an Arduino & high quality OS images is a golden combination for any hobby project! 
* To Thijs Elenbaas, creator of CmdMessenger. My favorite Arduino library. Higher level communication between C# & an Arduino within minutes! http://playground.arduino.cc/Code/CmdMessenger
* To Balázs Kelemen, creator of Arduino Softtimer. A great library for scheduling tasks on arduino. https://code.google.com/p/arduino-softtimer/
* To Armandob at the olimex.com forum for posting a ArmHf Mono & GTK# Deb file. This saved me a lot of mono compilation headache! ( https://www.olimex.com/forum/index.php?topic=2799.0 )
* To Solomon Peachy, creator of the libgutenprint DyeSub backend. Your addition to gutenprint in May 2014 was just the thing I needed to get the printer running! ( http://git.shaftnet.org/cgit/selphy_print.git/ )
* Froukje & Veerle (my wife & daughter) for being patient & planning the rest of the wedding while I was busy debugging the photobooth :)

### Scenario of my photobooth: ###
When the photobooth is connected to a power source, the UDOO automatically starts the photobooth application.
The application tries to detect the camera, and if it is connected, a big red dome button starts blinking inviting people to start a photo session.
When the red button is pushed, the camera takes 4 images which are directly shown on screen.
These 4 images are combined into a black&white collage which is stored on disk and subsequently shown in the UI.
When the photosession is finished, blinking 1P & 2P buttons indicate that the photo collage can directly be printed (1 print or 2 prints).
The prints are created on a 10×15 photo using a Canon dyesub printer. While the photobooth is printing another photosession can directly be started.

See: http://youtu.be/WWKsq6tpNGU for a demo.
And http://www.udoo.org/ProjectsAndTutorials/udoo-dslr-photobooth/?portfolioID=4028 for my UDoo project submission.

### Some details: ###
* The UDOO runs a debian wheezy linux ARMHF v1.1 image. http://www.udoo.org/downloads/ 
* The photobooth application is written in C# and runs on MONO 3.2.8 with a UI created in GTKSharp.
* A Nikon DSLR camera is used to interface with the UDOO using a managed wrapper of the libgphoto2 library. (any camera which supports capturing via PTP will do)
* Printing is done using CUPS which uses the recent LIBGutenprint52 libraries with dyesub spooler to print.
* Button clicks and lighting of the buttons is handled by the Arduino
* Communication between the C# application and the Arduino is done via the CmdMessenger 3 library over serial. (http://playground.arduino.cc/Code/CmdMessenger)
* The arduino sketch uses the SoftTimer library to allow running multiple tasks which are executed in intervals (blinking lights, reading serial, handling button clicks) (https://code.google.com/p/arduino-softtimer/)
* The lighted buttons are powered using simple BD135 transistors and PWM from the digital pins of the arduino.

### Required Hardware: ###
* DSLR camera supported by libgphoto2 (camera should support capturing via PTP)
* A Udoo board, or otherwise a PC / development board / virtual machine capable of running linux & an arduino connected to it.
* A (dyesub or other) photo printer

### Required software: ###
* Mono 3.2.8 or higher. I used the deb file provided at https://www.olimex.com/forum/index.php?topic=2799.0 
* libgphoto2: for interfacing with the camera via USB.  http://www.gphoto.org/proj/libgphoto2/
* libusb: for providing the usb interface to the printer & libgphoto2. http://www.libusb.org/ (The dyesub backend requires a new version, I used 1.0.9)
* If printing is enabled: Cups and LIBGutenprint 5.2. http://sourceforge.net/projects/gimp-print/files/gutenprint-5.2/5.2.10/
* If hardware buttons are enabled: an arduino http://www.arduino.cc/
* If running the GUI appliation: A monitor (fixed resolution of the GUI app is 1280x1024 for now!)

Note: The software was extensively tested on a UDOO board, but it should run on any x86/ARM board which supports running the above software.
The arduino code has been tested on the UDOO board / Arduino DUE and on an Arduino UNO & Arduino Leonardo.

Note: A serial connection is the typical way to connect the Arduino to the photobooth application, however 'keypress' emulation supported by the arduino Leonardo is also rudimentarily supported!

### How do I get set up? ###

T.B.D.

* Configuration
* Dependencies
* Deployment instructions

### Contribution guidelines ###

If you use this project and make changes, please commit them back to this repository or at least let me know.
Let me know up front before creating your own fork.

### Who do I talk to? ###

* Repo owner: pbronneberg

### License ###
  MIT License:
  
  Permission is hereby granted, free of charge, to any person obtaining
  a copy of this software and associated documentation files (the
  "Software"), to deal in the Software without restriction, including
  without limitation the rights to use, copy, modify, merge, publish,
  distribute, sublicense, and/or sell copies of the Software, and to
  permit persons to whom the Software is furnished to do so, subject to
  the following conditions:

  The above copyright notice and this permission notice shall be
  included in all copies or substantial portions of the Software.
  The original creator shall be notified 
  
  Copyright 2014 Patrick Bronneberg