#include <SoftTimer.h>
#include <SoftPwmTask.h>

#define INPUT_PIN_1 8
#define INPUT_PIN_2 9
#define INPUT_PIN_3 10
#define INPUT_PIN_4 11

/* In this demonstration a pulsating is implemented. */

#define OUT_PIN_1  4
#define OUT_PIN_2  5
#define OUT_PIN_3  6
#define OUT_PIN_4  7

// -- Set up PWM to the out pin.
//SoftPwmTask pwm1(OUT_PIN_1);
//SoftPwmTask pwm2(OUT_PIN_2);
//SoftPwmTask pwm3(OUT_PIN_3);
//SoftPwmTask pwm4(OUT_PIN_4);
//// -- This task will increment the PWM value. Will be called in every 50 milliseconds.
//Task incrementTask(40, increment);
//// -- This task will decrement the PWM value. Will be called in every 25 milliseconds.
//Task decrementTask(20, decrement);
//
//Task buttonTask1(20, buttonRead1);
//Task buttonTask2(20, buttonRead2);
//Task buttonTask3(20, buttonRead3);
//Task buttonTask4(20, buttonRead4);
//
//byte value = 0;
//const byte nrOfBlinks = 5;
//byte remainingBlinks = 0;
//
//// variables will change:
//int buttonState = 0;         // variable for reading the pushbutton status
//int wasPushed = 0;

void setup(void)
{
  pinMode(INPUT_PIN_1, INPUT);
  pinMode(INPUT_PIN_2, INPUT);
  pinMode(INPUT_PIN_3, INPUT);
  pinMode(INPUT_PIN_4, INPUT);
  pinMode(OUT_PIN_1, OUTPUT);
  pinMode(OUT_PIN_2, OUTPUT);
  pinMode(OUT_PIN_3, OUTPUT);
  pinMode(OUT_PIN_4, OUTPUT);
  
  // -- Register the PWM.
//  pwm1.analogWrite(0);
//  pwm2.analogWrite(0);
//  pwm3.analogWrite(0);
//  pwm4.analogWrite(0);
  //SoftTimer.add(&pwm1);
  //SoftTimer.add(&pwm2);
  //SoftTimer.add(&pwm3);
  //SoftTimer.add(&pwm4);
  //SoftTimer.add(&buttonTask1);
  //SoftTimer.add(&buttonTask2);
  //SoftTimer.add(&buttonTask3);
  //SoftTimer.add(&buttonTask4);
  
  //Serial.begin(9600);
  //Serial.println("---------------------------------------");
}

// the loop routine runs over and over again forever:
void loop() {
  digitalWrite(OUT_PIN_1, HIGH);   // turn the LED on (HIGH is the voltage level)
  digitalWrite(OUT_PIN_2, HIGH);   // turn the LED on (HIGH is the voltage level)
  digitalWrite(OUT_PIN_3, HIGH);   // turn the LED on (HIGH is the voltage level)
  digitalWrite(OUT_PIN_4, HIGH);   // turn the LED on (HIGH is the voltage level)
  delay(1000);               // wait for a second
  digitalWrite(OUT_PIN_1, LOW);    // turn the LED off by making the voltage LOW
  digitalWrite(OUT_PIN_2, LOW);    // turn the LED off by making the voltage LOW
  digitalWrite(OUT_PIN_3, LOW);    // turn the LED off by making the voltage LOW
  digitalWrite(OUT_PIN_4, LOW);    // turn the LED off by making the voltage LOW
  delay(1000);               // wait for a second
}

//void buttonRead(Task* me)
//{
//}
//
//void buttonRead(int inputPin, Task* buttonTask, Task* incrementTask)
//{
//  // read the state of the pushbutton value:
//  buttonState = digitalRead(inputPin);
//
//  // check if the pushbutton is pressed.
//  // if it is, the buttonState is HIGH:
//  if (buttonState == HIGH) {
//    if ( wasPushed == 0)
//    {
//     pwm.analogWrite(0);
//     Serial.println("Pushed");
//    wasPushed = 1; 
//    }
//  } 
//  else {
//    if (wasPushed == 1)
//    {
//      wasPushed = 0;
//  // -- Register the increment task.
//  remainingBlinks = nrOfBlinks;
//  SoftTimer.remove(&buttonTask);
//  SoftTimer.add(&incrementTask);
//  
//    } 
//  }
//    
//}
//
//void increment(Task* me) {
//  pwm.analogWrite(value);
//  value += 16;
//  if(value == 0) {
//    // -- Byte value overflows: 240 + 16 = 0
//    SoftTimer.remove(&incrementTask);
//    SoftTimer.add(&decrementTask);
//  }
//}
//
//
//void decrement(Task* me) {
//  value -= 16;
//  pwm.analogWrite(value);
//  if(value == 0) {
//    // -- Floor reached.
//    SoftTimer.remove(&decrementTask);
//    remainingBlinks = remainingBlinks -1;
//    if (remainingBlinks > 0)
//    {
//      SoftTimer.add(&incrementTask);
//    }
//    else
//    {
//      SoftTimer.add(&buttonTask);
//    }
//  }
//}

