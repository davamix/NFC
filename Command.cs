using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nfc
{
    public abstract class Command
    {
        public byte[] Execute(SerialPort port)
        {
            byte[] buffer = null;
            var command = GenerateCommand();
            Console.WriteLine(string.Format("Sending command: {0}", BitConverter.ToString(command)));

            port.Write(command, 0, command.Length);

            Thread.Sleep(500);
            while (port.BytesToRead == 0) { }

            while (port.BytesToRead > 0)
            {
                buffer = new byte[port.BytesToRead];
                port.Read(buffer, 0, port.BytesToRead);
                Console.Write(string.Format("Read: {0}", BitConverter.ToString(buffer)));
            }
            Console.WriteLine();
            return buffer;
        }

        protected abstract byte[] GenerateCommand();
    }
}