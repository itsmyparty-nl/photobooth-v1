// *** PhotoboothController ***
// Control the photobooth by supplying buttons which can be armed and released,
// which send commands to the application via a serial interface and the
// CmdMessenger library
// Based on the ArduinoController example

#include <CmdMessenger.h>  // CmdMessenger
#include <SoftTimer.h>
#include <SoftPwmTask.h>

// Blinking led variables 
const int kBlinkLed            = 13;  // Pin of internal Led
bool ledState                  = 1;   // Current state of Led
float ledFrequency             = 1.0; // Current blink frequency of Led
unsigned long intervalOn;
unsigned long intervalOff;
unsigned long prevBlinkTime = 0;

bool triggerBtnPushed = false;
bool printBtnPushed = false;
bool powerBtnPushed = false;

#define INPUT_PIN_1 8
#define INPUT_PIN_2 9
#define INPUT_PIN_3 10
#define INPUT_PIN_4 11

/* In this demonstration a pulsating is implemented. */
#define OUT_PIN_1  4
#define OUT_PIN_2  5
#define OUT_PIN_3  6
#define OUT_PIN_4  7

#define triggerBtnLed  OUT_PIN_1
#define powerBtnLed  OUT_PIN_2
#define printBtnLed  OUT_PIN_3

#define triggerBtnPin  INPUT_PIN_1
#define powerBtnPin  INPUT_PIN_2
#define printBtnPin  INPUT_PIN_3

#define TRIGGERBUTTON 60
#define POWERBUTTON 70
#define PRINTBUTTON 80

Task buttonTaskTrigger(20, buttonReadTrigger);
Task buttonTaskPrint(20, buttonReadPrint);
Task buttonTaskPower(20, buttonReadPower);
Task commandHandlerTask(20, readSerial);

// Attach a new CmdMessenger object to the default Serial port
CmdMessenger cmdMessenger = CmdMessenger(Serial);

// This is the list of recognized commands. These kcan be commands that can either be sent or received. 
// In order to receive, attach a callback function to these events
enum
{
  kAcknowledge,
  kError,
  kPower,
  kTrigger,
  kPrint,
  kPrepareControl,
  kReleaseControl
};

// Callbacks define on which received commands we take action
void attachCommandCallbacks()
{
  // Attach callback methods
  cmdMessenger.attach(OnUnknownCommand);
  cmdMessenger.attach(kPrepareControl, OnPrepareControl);
  cmdMessenger.attach(kReleaseControl, OnReleaseControl);
}

// Called when a received command has no attached function
void OnUnknownCommand()
{
  cmdMessenger.sendCmd(kError,"Command without attached callback");
}

// Callback function that sets led on or off
void OnPrepareControl()
{
  int control;
  // Read led state argument, interpret string as int
  control = cmdMessenger.readIntArg();
  switch (control)
  {
    case TRIGGERBUTTON:
      SoftTimer.remove(&buttonTaskTrigger);
      digitalWrite(triggerBtnLed, HIGH);
      break;
    case POWERBUTTON:
      SoftTimer.remove(&buttonTaskPower);
      digitalWrite(powerBtnLed, HIGH);
      break;
    case PRINTBUTTON:
      SoftTimer.remove(&buttonTaskPrint);
      digitalWrite(printBtnLed, HIGH);
      break;
    default:
      cmdMessenger.sendCmd(kError,"Unsupported button");
  }

  cmdMessenger.sendCmd(kAcknowledge,control);
}

// Callback function that sets led on or off
void OnReleaseControl()
{
  int control;
  // Read led state argument, interpret string as string
  control = cmdMessenger.readIntArg();
  switch (control)
  {
    case TRIGGERBUTTON:
      SoftTimer.remove(&buttonTaskTrigger);
      digitalWrite(triggerBtnLed, LOW);
      break;
    case POWERBUTTON:
      SoftTimer.remove(&buttonTaskPower);
      digitalWrite(powerBtnLed, LOW);
      break;
    case PRINTBUTTON:
      SoftTimer.remove(&buttonTaskPrint);
      digitalWrite(printBtnLed, LOW);
      break;
    default:
      cmdMessenger.sendCmd(kError,"Unsupported button");
  }
  cmdMessenger.sendCmd(kAcknowledge,control);
}

// Setup function
void setup() 
{
  pinMode(INPUT_PIN_1, INPUT);
  pinMode(INPUT_PIN_2, INPUT);
  pinMode(INPUT_PIN_3, INPUT);
  pinMode(INPUT_PIN_4, INPUT);
  pinMode(OUT_PIN_1, OUTPUT);
  pinMode(OUT_PIN_2, OUTPUT);
  pinMode(OUT_PIN_3, OUTPUT);
  pinMode(OUT_PIN_4, OUTPUT);
  
  // Listen on serial connection for messages from the PC
  Serial.begin(115200); 

  // Adds newline to every command
  //cmdMessenger.printLfCr();   

  // Attach my application's user-defined callback methods
  attachCommandCallbacks();

  // Send the status to the PC that says the Arduino has booted
  // Note that this is a good debug function: it will let you also know 
  // if your program had a bug and the arduino restarted  
  cmdMessenger.sendCmd(kAcknowledge,"Arduino has started!");

  // set pin for blink LED
  pinMode(kBlinkLed, OUTPUT);
  
  SoftTimer.add(&commandHandlerTask);
}

// Loop function
void readSerial(Task* me) 
{
  // Process incoming serial data, and perform callbacks
  cmdMessenger.feedinSerialData();
  blinkLed();
}

void buttonReadTrigger(Task* me)
{
  readButton(triggerBtnPin,kTrigger, &triggerBtnPushed);
}

void buttonReadPrint(Task* me)
{
  readButton(printBtnPin,kPrint, &printBtnPushed);
}

void buttonReadPower(Task* me)
{
  readButton(powerBtnPin,kPower, &powerBtnPushed);
}

// Returns if it has been more than interval (in ms) ago. Used for periodic actions
bool blinkLed() {
  if (  millis() - prevBlinkTime > intervalOff ) {
    // Turn led off during halfway interval
    prevBlinkTime = millis();
    digitalWrite(kBlinkLed, LOW);
  } else if (  millis() - prevBlinkTime > intervalOn ) {
    // Turn led on at end of interval (if led state is on)
    digitalWrite(kBlinkLed, ledState?HIGH:LOW);
  } 
}

void readButton(int inputPin, int buttonCommand, bool* wasPushed)
{
  // read the state of the pushbutton value:
  int buttonState = digitalRead(inputPin);

  // check if the pushbutton is pressed.
  // if it is, the buttonState is HIGH:
  if (buttonState == HIGH) {
    if ( *wasPushed == false)
    {
     cmdMessenger.sendCmd(buttonCommand, 0);
     
    *wasPushed = true; 
    }
  } 
  else {
    if (*wasPushed == true)
    {
      *wasPushed = false;
    } 
  }
}

////HEARTBEAT BLINKING
//const int nrOfBlinks = 4;
//
//void startHeartbeat(Task* buttonTask, Task* incrementTask)
//{
//  // -- Register the increment task.
//  remainingBlinks = nrOfBlinks;
//  SoftTimer.remove(&buttonTask);
//  SoftTimer.add(&incrementTask);
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
