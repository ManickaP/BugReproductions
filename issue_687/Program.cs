using System;
using RJCP.IO.Ports;
using System.IO.Ports;

namespace issue_687
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPortStream port = new SerialPortStream("/dev/ttyS0", 115200);
            port.Open();
            port.Close();
            SerialPort port2 = new SerialPort("/dev/ttyS0");
            port2.Open();
            port2.Close();
            /*var buf = new byte[5];
            var l = port.Read(buf, 0, 5);
            Console.WriteLine("Bytes read: {0}", l);
            Console.WriteLine("Content: {0}", BitConverter.ToString(buf).Replace("-", ","));
            port.Close();*/
        }
    }
}
