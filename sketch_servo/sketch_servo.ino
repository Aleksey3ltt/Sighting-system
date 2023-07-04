#include <Servo.h>
Servo servo1, servo2;

char c;
String dataIn;
uint8_t servo1Deg, servo2Deg, indexOfA, indexOfB;

void setup() 
{
  // put your setup code here, to run once:
  Serial.begin (9600);
  servo1.attach(13);
  servo2.attach(12);
}

void loop() 
{
  // put your main code here, to run repeatedly:
  Recive_Serial_Data();
  if (c=='\n')
  {
     Parse_the_data();
     c=0;
     dataIn="";
  }
  servo1.write(servo1Deg);
  servo2.write(servo2Deg);
}

void Recive_Serial_Data()
{
  while(Serial.available()>0)
  {
    c=Serial.read();
    if (c=='\n')
    {
      break;
    }
    else
    {
      dataIn+=c;
    }
  }
}

void Parse_the_data()
{
  String stringServo1Deg, stringServo2Deg;
  indexOfA=dataIn.indexOf("A");
  indexOfB=dataIn.indexOf("B");
  if (indexOfA>-1)
  {
    stringServo1Deg = dataIn.substring(0, indexOfA);
    servo1Deg = stringServo1Deg.toInt();
  }
  if (indexOfB>-1)
  {
    stringServo2Deg = dataIn.substring(indexOfA+1, indexOfB);
    servo2Deg = stringServo2Deg.toInt();    
  }
}
