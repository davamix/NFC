using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nfc
{
    internal class Program
    {
        private static byte[] searchCard = { 0x03, 0x02, 0x00 };
        private static byte[] anticollision = { 0x02, 0x03 };
        private static byte[] selectCard = { 0x02, 0x04 };
        private static byte[] stopCard = { 0x02, 0x09 };

        private static byte[] downloadKeyA =
        {
            0x0A, //Number of bytes
            0x0B, //Download key command
            0x00, //Download mode: Key A (0x01 for Key B)
            0x00, //Sector 0 (0x0F for sector 15)
            0xFF, //Key start
            0xFF,
            0xFF,
            0xFF,
            0xFF,
            0xFF //Key End
        };

        private static byte[] writeCommand = { 0x13, 0x07, 0x01, 0xAA, 0x76, 0x96, 0x0A, 0x33, 0x08, 0x04, 0x00, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69 };

        private static byte[] readData = { 0x03, 0x06, 0x01 };
        private static byte[] verifyKeyA = { 0x04, 0x05, 0x00, 0x00 };

        private static void Main(string[] args)
        {
            var port = new SerialPort("COM3");

            if (port.IsOpen == false)
                port.Open();

            port.DiscardInBuffer();
            port.DiscardOutBuffer();

            ExecuteCommand(port, searchCard);
            ExecuteCommand(port, anticollision);
            ExecuteCommand(port, selectCard);
            ExecuteCommand(port, downloadKeyA);
            ExecuteCommand(port, verifyKeyA);
            ExecuteCommand(port, readData);
            ExecuteCommand(port, writeCommand);
            ExecuteCommand(port, readData);

            port.Close();
            Console.ReadLine();
        }

        private static void ExecuteCommand(SerialPort port, byte[] command)
        {
            var fullCommand = AddCrc(command);
            Console.WriteLine(string.Format("Write: {0}", BitConverter.ToString(fullCommand)));

            port.Write(fullCommand, 0, fullCommand.Length);
            Thread.Sleep(500);
            while (port.BytesToRead == 0)
            {
            }

            while (port.BytesToRead > 0)
            {
                var buffer = new byte[port.BytesToRead];
                port.Read(buffer, 0, port.BytesToRead);
                Console.Write(string.Format("Read: {0}", BitConverter.ToString(buffer)));
            }
            Console.WriteLine();
        }

        private static byte[] AddCrc(byte[] command)
        {
            var result = new byte[command.Length + 1];

            Array.Copy(command, result, command.Length);
            result[command.Length] = Crc8.ComputeChecksum(command);
            return result;
        }
    }

    public static class Crc8
    {
        private static byte[] table = new byte[256];

        // x8 + x7 + x6 + x4 + x2 + 1
        private const byte poly = 0xd5;

        public static byte ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                {
                    crc = (byte)(crc + b);
                }
            }
            return crc;
        }
    }
}