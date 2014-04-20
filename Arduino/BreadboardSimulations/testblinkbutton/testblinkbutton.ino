#include <SoftTimer.h>
#include <SoftPwmTask.h>

#define INPUT_PIN 8

/* In this demonstration a pulsating is implemented. */

#define OUT_PIN  12

// -- Set up PWM to the out pin.
SoftPwmTask pwm(OUT_PIN);
// -- This task will increment the PWM value. Will be called in every 50 milliseconds.
Task incrementTask(40, increment);
// -- This task will decrement the PWM value. Will be called in every 25 milliseconds.
Task decrementTask(20, decrement);

Task buttonTask(20, buttonRead);

byte value = 0;
const byte nrOfBlinks = 5;
byte remainingBlinks = 0;

// variables will change:
int buttonState = 0;         // variable for reading the pushbutton status
int wasPushed = 0;

void setup(void)
{
  pinMode(INPUT_PIN, INPUT);
  pinMode(OUT_PIN, OUTPUT);
  
  // -- Register the PWM.
  pwm.analogWrite(255);
  SoftTimer.add(&pwm);
  SoftTimer.add(&buttonTask);
  
  Serial.begin(9600);
  Serial.println("---------------------------------------");
}

void buttonRead(Task* me)
{
  // read the state of the pushbutton value:
  buttonState = digitalRead(INPUT_PIN);

  // check if the pushbutton is pressed.
  // if it is, the buttonState is HIGH:
  if (buttonState == HIGH) {
    if ( wasPushed == 0)
    {
     pwm.analogWrite(0);
     Serial.println("Pushed");
    wasPushed = 1; 
    }
  } 
  else {
    if (wasPushed == 1)
    {
      wasPushed = 0;
  // -- Register the increment task.
  remainingBlinks = nrOfBlinks;
  SoftTimer.remove(&buttonTask);
  SoftTimer.add(&incrementTask);
  
    } 
  }
    
}

void increment(Task* me) {
  pwm.analogWrite(value);
  value += 16;
  if(value == 0) {
    // -- Byte value overflows: 240 + 16 = 0
    SoftTimer.remove(&incrementTask);
    SoftTimer.add(&decrementTask);
  }
}


void decrement(Task* me) {
  value -= 16;
  pwm.analogWrite(value);
  if(value == 0) {
    // -- Floor reached.
    SoftTimer.remove(&decrementTask);
    remainingBlinks = remainingBlinks -1;
    if (remainingBlinks > 0)
    {
      SoftTimer.add(&incrementTask);
    }
    else
    {
      SoftTimer.add(&buttonTask);
    }
  }
}

