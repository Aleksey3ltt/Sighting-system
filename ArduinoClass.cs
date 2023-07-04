using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sighting_system
{
    internal class ArduinoClass
    {
        int delay90 = 3000;
        int delay = 40;
        int pos0X = 90;
        int pos0Y = 15;
        int posXmax = 30;
        int posYmax = 30;
        SerialPort serialPort = null;

        public void InitPort()
        {
            if (serialPort == null)
            {
                serialPort = new SerialPort("COM4", 9600);      //Set your board COM
                serialPort.Open();
            }
        }

        public void SetInitialAngles()
        {
            serialPort.Write(pos0X + "A" + pos0Y + "B" + "\n");
            var t = Task.Run(async delegate { await Task.Delay(delay90); }); t.Wait();
        }

        public void SetCalibration()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                SetInitialAngles();

                for (int i = 0; i < posXmax; i++)
                {
                    int servo1 = pos0X - i;
                    int servo2 = i * posYmax / posXmax + pos0Y;
                    var t1 = Task.Run(async delegate { await Task.Delay(delay); }); t1.Wait();
                    serialPort.Write(servo1 + "A" + servo2 + "B" + "\n");
                }
                for (int i = 0; i < 2 * posXmax; i++)
                {
                    int servo1 = i + pos0X - posXmax;
                    int servo2 = posYmax + pos0Y;
                    var t1 = Task.Run(async delegate { await Task.Delay(delay); }); t1.Wait();
                    serialPort.Write(servo1 + "A" + servo2 + "B" + "\n");
                }
                for (int i = 0; i < posXmax; i++)
                {
                    int servo1 = -i + pos0X + posXmax;
                    int servo2 = posYmax + pos0Y - i;
                    var t1 = Task.Run(async delegate { await Task.Delay(delay); }); t1.Wait();
                    serialPort.Write(servo1 + "A" + servo2 + "B" + "\n");
                }
            }
            else
            {
                InitPort();
            }
        }
        public void Targeting(int deltaX, int deltaY)
        {
            int viewingAngle = 67;
            double factor = 0.25 * viewingAngle / 640;
            double angleX = Math.Ceiling(Math.Abs(factor * deltaX));
            double angleY = Math.Ceiling(Math.Abs(factor * deltaY));
            int posX = Convert.ToInt32(angleX);
            int posY = Convert.ToInt32(angleY);
            int maxPos = Math.Max(posX, posY);
            int delay = 40;
            double servo1;
            double servo2;

            for (int i = 0; i <= maxPos; i++)
            {
                if (maxPos == posX)
                {
                    servo1 = pos0X + i * Math.Sign(deltaX);
                }
                else
                {
                    servo1 = pos0X + Math.Sign(deltaX) * posX * i / maxPos;
                }
                if (maxPos == posY)
                {
                    servo2 = pos0Y + i * Math.Sign(deltaY);
                }
                else
                {
                    servo2 = pos0Y + Math.Sign(deltaY) * posY * i / maxPos;
                }
               
                serialPort.Write(servo1 + "A" + servo2 + "B" + "\n");
                var t2 = Task.Run(async delegate { await Task.Delay(delay); }); t2.Wait();
            }

            var t3 = Task.Run(async delegate { await Task.Delay(6000); }); t3.Wait();
        }

        public void ClosePort()
        {
            if (serialPort != null)
            {
                serialPort.Close();
                serialPort = null;
            }
        }
    }
}