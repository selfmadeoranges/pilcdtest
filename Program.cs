using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;


namespace BusStopPi
{
    class Program
    {
        static void Main(string[] args)
        {
            byte [] rain1 =
            {
                0b00000,
                0b00001,
                0b00010,
                0b01100,
                0b10000,
                0b10000,
                0b01000,
                0b00111
            };

            byte[] rain2 =
            {
                0b11111,
                0b00000,
                0b00000,
                0b00000,
                0b00000,
                0b00000,
                0b00000,
                0b11111
            };

            byte[] rain3 =
            {
                0b00000,
                0b10000,
                0b01000,
                0b00110,
                0b00001,
                0b00001,
                0b00010,
                0b11100
            };

            byte[] rain4 =
            {
                0b00100,
                0b00100,
                0b00100,
                0b00001,
                0b00001,
                0b00001,
                0b00000,
                0b00000
            };

            byte[] rain5 =
            {
                0b00000,
                0b00100,
                0b00100,
                0b00100,
                0b00000,
                0b00000,
                0b00000,
                0b00000
            };

            byte[] rain6 =
            {
                0b01001,
                0b01001,
                0b00001,
                0b00100,
                0b00100,
                0b00100,
                0b00000,
                0b00000
            };

            using I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
            using var driver = new Pcf8574(i2c);

            using var lcd = new Lcd1602(registerSelectPin: 0,
                                    enablePin: 2,
                                    dataPins: new int[] { 4, 5, 6, 7 },
                                    backlightPin: 3,
                                    backlightBrightness: 0.1f,
                                    readWritePin: 1,
                                    controller: new GpioController(PinNumberingScheme.Logical, driver));

            BusAPI busAPI = new BusAPI();
            WeatherAPI wAPI = new WeatherAPI();
            
            RECV recv = RECV.FAILED;
            RECVW recvw = RECVW.FAILED;

            int bus1_m, bus1_s;
            int bus2_m;

            while (true)
            {
                recv = busAPI.GetBusStopData("11050601005", "1100061527").GetAwaiter().GetResult();
                recvw = wAPI.GetNowWeather().GetAwaiter().GetResult();

                bus1_m = busAPI.arrival1 / 60;
                bus1_s = busAPI.arrival1 % 60;

                bus2_m = busAPI.arrival2 / 60;

                lcd.Clear();
                lcd.SetCursorPosition(0, 0);
                lcd.Write("BUS :" + bus1_m.ToString() + "m" + bus1_s.ToString() + "s" +"/"+ bus2_m.ToString() + "m");
                lcd.SetCursorPosition(0, 1);
                lcd.Write("Temp: " + wAPI.temp + "`C");
                //currentLine = (currentLine == 3) ? 0 : currentLine + 1;
                Thread.Sleep(10000);
            }


        }
    }
}
