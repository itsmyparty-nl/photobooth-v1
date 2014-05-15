// *** PhotoboothController ***
// Control the photobooth by supplying buttons which can be armed and released,
// which send commands to the application via a serial interface and the
// CmdMessenger library
// Based on the ArduinoController example

#include <CmdMessenger.h>  // CmdMessenger
#include <SoftTimer.h>
#include <HeartBeat.h>
#include <BlinkTask.h>

// status heartbeat led variables 
const int ledPin            = 13;  // Pin of internal Led
bool ledState               = 1;   // Current state of Led

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

Task buttonTaskTrigger(110, buttonReadTrigger);
Task buttonTaskPrint(80, buttonReadPrint);
Task buttonTaskPower(90, buttonReadPower);
Task commandHandlerTask(500, readSerial);

Heartbeat triggerLedTask(triggerBtnLed);
BlinkTask printLedTask(printBtnLed);
BlinkTask powerLedTask(powerBtnLed);

// Attach a new CmdMessenger object to the default Serial port
CmdMessenger cmdMessenger = CmdMessenger(Serial);

// This is the list of recognized commands. These kcan be commands that can either be sent or received. 
// In order to receive, attach a callback function to these events
enum
{
  kAcknowledge,
  kError,
  kTrigger,
  kPrint,
  kPower,
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
    case kTrigger:
      SoftTimer.add(&buttonTaskTrigger);
      SoftTimer.add(&triggerLedTask);
      break;
    case kPower:
      SoftTimer.add(&buttonTaskPower);
      digitalWrite(powerBtnLed, HIGH);
      break;
    case kPrint:
      SoftTimer.add(&buttonTaskPrint);
      SoftTimer.add(&printLedTask);
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
    case kTrigger:
      SoftTimer.remove(&buttonTaskTrigger);
      SoftTimer.remove(&triggerLedTask);
      digitalWrite(triggerBtnLed, LOW);
      break;
    case kPower:
      SoftTimer.remove(&buttonTaskPower);
      digitalWrite(powerBtnLed, LOW);
      break;
    case kPrint:
      SoftTimer.remove(&buttonTaskPrint);
      SoftTimer.remove(&printLedTask);
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
  // Wait until the serial connection is present
  //while (!Serial) ;
  // Adds newline to every command
  //cmdMessenger.printLfCr();   

  // Attach my application's user-defined callback methods
  attachCommandCallbacks();

  // Send the status to the PC that says the Arduino has booted
  // Note that this is a good debug function: it will let you also know 
  // if your program had a bug and the arduino restarted  
  cmdMessenger.sendCmd(kAcknowledge,"Arduino has started!");

  // set pin for blink LED
  pinMode(ledPin, OUTPUT);
  
  SoftTimer.add(&commandHandlerTask);
}

// Loop function
void readSerial(Task* me) 
{
  // Process incoming serial data, and perform callbacks
  cmdMessenger.feedinSerialData();
  // Show a heartbeat to know that command processing is not frozen
  ledState = ledState ? LOW :HIGH;
  digitalWrite(ledPin, ledState);
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
