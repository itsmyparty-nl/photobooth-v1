### Photobooth Repo ###

* A photobooth initially created using the UDOO development board, later ported to run on any linux system.
* Written in C# .Net 9

### Description: ###
I created this for my wedding and we had tons of fun with it (both building it, and taking pictures during the wedding).
The photobooth is fully based on open source components, and would not have been possible without the open source software & hardware community.
The full source code is my thank you to the community. Hope you have as much fun with it as we did!

The project ended up as winner of the Udoo Call for Projects: http://www.udoo.org/the-king-of-udooers-wholl-fly-to-rome/

Note: A few libraries (CmdMessenger & Libgphoto2) are currently integrated in the project as source code.

### Big thank you ###
* To the Arduino people: Without Arduino I never would have used micro controllers! Thanks for starting the open-source hardware madness!
* To Udoo: Combining a powerful ARM board with an Arduino & high quality OS images is a golden combination for any hobby project! 
* To Thijs Elenbaas, creator of CmdMessenger. My favorite Arduino library. Higher level communication between C# & an Arduino within minutes! http://playground.arduino.cc/Code/CmdMessenger
* To Balázs Kelemen, creator of Arduino Softtimer. A great library for scheduling tasks on arduino. https://code.google.com/p/arduino-softtimer/
* To Armandob at the olimex.com forum for posting a ArmHf Mono & GTK# Deb file. This saved me a lot of mono compilation headache! ( https://www.olimex.com/forum/index.php?topic=2799.0 )
* To Solomon Peachy, creator of the libgutenprint DyeSub backend. Your addition to gutenprint in May 2014 was just the thing I needed to get the printer running! ( http://git.shaftnet.org/cgit/selphy_print.git/ )
* Froukje & Veerle (my wife & daughter) for being patient & planning the rest of the wedding while I was busy debugging the photobooth :)

### Scenario of my photobooth: ###
When the photobooth is connected to a power source, the system automatically starts the photobooth application.
The application tries to detect the camera, and if it is connected, a big red dome button starts blinking inviting people to start a photo session.
When the red button is pushed, the camera takes 4 images which are directly shown on screen.
These 4 images are combined into a collage which is stored on disk and subsequently shown in the UI.
When the photosession is finished, the user can choose to print the collage, or to start a new photosession.

Pictures are automatically offloaded from the photobooth to cloud storage, and can be viewed on the [photobooth.itsmyparty.nl](https://photobooth.itsmyparty.nl) website.

See: http://youtu.be/WWKsq6tpNGU for a demo.
And http://www.udoo.org/ProjectsAndTutorials/udoo-dslr-photobooth/?portfolioID=4028 for my UDoo project submission.

### Some details: ###
* Photobooth is built for Canonical based linux distributions (currently tested on Ubuntu Server 24.04)
* The photobooth application is written in C# with a UI created in GTKSharp.
* A DSLR camera is used to interface with the system using a managed wrapper of the libgphoto2 library. (any camera which supports capturing via PTP will do)
* Button clicks and lighting of the buttons is handled by the Arduino
* Communication between the C# application and the Arduino is done via the CmdMessenger 3 library over serial. (http://playground.arduino.cc/Code/CmdMessenger)
* The arduino sketch uses the SoftTimer library to allow running multiple tasks which are executed in intervals (blinking lights, reading serial, handling button clicks) (https://code.google.com/p/arduino-softtimer/)
* The lighted buttons are powered using simple BD135 transistors and PWM from the digital pins of the arduino.

### Required Hardware: ###
* DSLR camera supported by libgphoto2 (camera should support capturing via PTP)
* A development board, raspberry pi, or otherwise a PC capable of running linux & connecting an arduino to it via serial.

### Required software: ###
All required software is installed via the scripts in the [scripts](./scripts) folder.

### How do I get set up? ###
Execute the scripts in the [scripts](./scripts) folder to install all required software & build and install the photobooth application.

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