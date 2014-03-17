#include <SoftTimer.h>
#include <SoftPwmTask.h>
#include <PciManager.h>
#include <Debouncer.h>

#define INPUT_PIN 2

/* In this demonstration a pulsating is implemented. */

#define OUT_PIN  13

// -- Set up PWM to the out pin.
SoftPwmTask pwm(OUT_PIN);
// -- This task will increment the PWM value. Will be called in every 50 milliseconds.
Task incrementTask(40, increment);
// -- This task will decrement the PWM value. Will be called in every 25 milliseconds.
Task decrementTask(20, decrement);

Debouncer debouncer(INPUT_PIN, MODE_CLOSE_ON_PUSH, onPressed, onReleased);

byte value = 0;
const byte nrOfBlinks = 5;
byte remainingBlinks = 0;

void setup(void)
{
  PciManager.registerListener(INPUT_PIN, &debouncer);
  
  // -- Register the PWM.
  pwm.analogWrite(255);
  SoftTimer.add(&pwm);
}

void onPressed() {
  pwm.analogWrite(0);
}

void onReleased(unsigned long pressTimespan) {
  // -- Register the increment task.
  remainingBlinks = nrOfBlinks;
  SoftTimer.add(&incrementTask);
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
  }
}

