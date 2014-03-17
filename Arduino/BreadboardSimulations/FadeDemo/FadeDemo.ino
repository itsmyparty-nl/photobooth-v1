/*
 Fade
 
 This example shows how to fade an LED on pin 9
 using the analogWrite() function.
 
 This example code is in the public domain.
 */

int led = 9;           // the pin that the LED is attached to
int brightness = 0;    // how bright the LED is
int fadeAmount = 5;    // how many points to fade the LED by
const int buttonPin = 2;     // the number of the pushbutton pin
// variables will change:
int buttonState = 0;         // variable for reading the pushbutton status

// the setup routine runs once when you press reset:
void setup()  { 
  // declare pin 9 to be an output:
  pinMode(led, OUTPUT);
  pinMode(buttonPin, INPUT);
} 

// the loop routine runs over and over again forever:
void loop()  { 
  // read the state of the pushbutton value:
  buttonState = digitalRead(buttonPin);
  
  if (buttonState == HIGH) {
  // set the brightness of pin 9:
  analogWrite(led, brightness);    

  // change the brightness for next time through the loop:
  brightness = brightness + fadeAmount;

  // reverse the direction of the fading at the ends of the fade: 
  if (brightness == 0 || brightness == 255) {
    fadeAmount = -fadeAmount ; 
  }     
  // wait for 30 milliseconds to see the dimming effect    
  delay(30);
  }
  if (buttonState == LOW) {
      analogWrite(led, 0);
  }  
}

