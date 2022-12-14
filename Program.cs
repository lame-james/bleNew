using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace bleNew
{
    internal class Program
    {
        static SerialPort _serialPort;
        
        static async Task Main(string[] args)
        {
            while (true)
            {
                BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync("BluetoothLE#BluetoothLEc8:09:a8:1d:d5:1b-e7:8b:3a:bc:7d:53");
                GattDeviceServicesResult result;
                do
                {
                    Console.WriteLine("Attempting connection...");
                    result = await bluetoothLeDevice.GetGattServicesAsync();
                }
                while (result.Status != GattCommunicationStatus.Success);

                Console.WriteLine("Successful pairing with: Pavlok");
                var services = result.Services;
                foreach (var service in services)
                {
                    if(service.Uuid.ToString().Equals("156e1000-a300-4fea-897b-86f698d74461")) 
                    { 
                        GattCharacteristicsResult characteristicResult = await service.GetCharacteristicsAsync();

                        if (characteristicResult.Status == GattCommunicationStatus.Success)
                        {
                            var characteristics = characteristicResult.Characteristics;
                            GattCharacteristic vibeChar = null;
                            GattCharacteristic beepChar = null;
                            GattCharacteristic zapChar = null;

                            foreach (var characteristic in characteristics)
                            {
                                if (characteristic.Uuid.ToString().Equals("00001001-0000-1000-8000-00805f9b34fb"))
                                {
                                    vibeChar = characteristic;
                                }
                                else if (characteristic.Uuid.ToString().Equals("00001002-0000-1000-8000-00805f9b34fb"))
                                {
                                    beepChar = characteristic;
                                }
                                else if (characteristic.Uuid.ToString().Equals("00001003-0000-1000-8000-00805f9b34fb"))
                                {
                                    zapChar = characteristic;
                                }
                            }

                            _serialPort = new SerialPort();
                            _serialPort.PortName = "COM3";
                            _serialPort.BaudRate = 9600;
                            _serialPort.Open();

                            Console.WriteLine("Ready to go!");

                            var writer = new DataWriter();

                            writer.WriteByte(0x81);
                            writer.WriteByte(0x00);
                            writer.WriteByte(0x32);
                            writer.WriteByte(0x16);
                            writer.WriteByte(0x16);

                            try
                            {
                                GattCommunicationStatus writeResult = await beepChar.WriteValueAsync(writer.DetachBuffer());
                                if (writeResult == GattCommunicationStatus.Success)
                                {
                                    Console.WriteLine("Beeped!");
                                }
                            }
                            catch (System.Exception e)
                            {

                            }

                            while (true)
                            {
                                string a = _serialPort.ReadExisting();                                
                                    
                                if(!string.IsNullOrEmpty(a))
                                {
                                    writer = new DataWriter();
                                    writer.WriteByte(0x81);
                                    writer.WriteByte(0x02);
                                    writer.WriteByte(0x32);
                                    writer.WriteByte(0x16);
                                    writer.WriteByte(0x16);

                                    try
                                    {
                                        GattCommunicationStatus writeResult = await vibeChar.WriteValueAsync(writer.DetachBuffer());
                                        if (writeResult == GattCommunicationStatus.Success)
                                        {
                                            Console.WriteLine("Vibrate!");
                                        }
                                    }
                                    catch (System.Exception e)
                                    {

                                    }

                                }

                                Thread.Sleep(200);
                            }

                            /*
                            int run = 1;
                            do
                            {
                                Console.WriteLine("1 - Vibrate\n2 - Beep\n3 - Zap\n0 - exit");
                                String input = Console.ReadLine();
                                run = Convert.ToInt32((string)input);

                                if (run == 1)
                                {
                                    var writer = new DataWriter();
                                    writer.WriteByte(0x81);
                                    writer.WriteByte(0x02);
                                    writer.WriteByte(0x32);
                                    writer.WriteByte(0x16);
                                    writer.WriteByte(0x16);

                                    try
                                    {
                                        GattCommunicationStatus writeResult = await vibeChar.WriteValueAsync(writer.DetachBuffer());
                                        if (writeResult == GattCommunicationStatus.Success)
                                        {
                                            Console.WriteLine("Vibrate!");
                                        }
                                    }
                                    catch (System.Exception e)
                                    {

                                    }
                                }
                                else if (run == 2)
                                {
                                    var writer = new DataWriter();

                                    writer.WriteByte(0x81);
                                    writer.WriteByte(0x00);
                                    writer.WriteByte(0x32);
                                    writer.WriteByte(0x16);
                                    writer.WriteByte(0x16);

                                    try
                                    {
                                        GattCommunicationStatus writeResult = await beepChar.WriteValueAsync(writer.DetachBuffer());
                                        if (writeResult == GattCommunicationStatus.Success)
                                        {
                                            Console.WriteLine("Beeped!");
                                        }
                                    }
                                    catch (System.Exception e)
                                    {

                                    }
                                }
                                else if (run == 3)
                                {
                                    var writer = new DataWriter();

                                    writer.WriteByte(0x89);
                                    writer.WriteByte(0x46);

                                    try
                                    {
                                        GattCommunicationStatus writeResult = await zapChar.WriteValueAsync(writer.DetachBuffer());
                                        if (writeResult == GattCommunicationStatus.Success)
                                        {
                                            Console.WriteLine("Zapped!");
                                        }
                                    }
                                    catch (System.Exception e)
                                    {

                                    }
                                }
                            }
                            while (run != 0);*/
                        }
                    }
                }
                Console.WriteLine("Press any key to exit application");
                Console.ReadKey();
                break;
            }
        }
    }
}
