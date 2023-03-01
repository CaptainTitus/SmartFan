
using Iot.Device.DHTxx.Esp32;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.Threading;

namespace ESP32Test
{
    public class Program
    {
        private static int _interval = 545;
        private static double _maxTemperature = 23;
        private static GpioController _gpio;
        private static GpioPin _red;
        private static GpioPin _blue;
        private static GpioPin _green;
        private static GpioPin _relaySwitch;

        public static void Main()
        {
            SetupGpios();

            MonitorTemperature();
        }

        private static void MonitorTemperature()
        {
            using (Dht11 dht = new(17, 16))
            {
                while (true)
                {

                    var temperature = dht.Temperature.DegreesCelsius;
                    var humidity = dht.Humidity;

                    FlashLED(_red, _green, _blue);

                    if (dht.IsLastReadSuccessful)
                    {
                        Debug.WriteLine($"Temp: {temperature}");
                        Debug.WriteLine($"Humidity: {humidity.Percent}");

                        //Tens 25.1 = 2
                        FlashFor(_red, (int)(temperature / 10));
                        //ones 25.1 = 5
                        FlashFor(_green, (int)(temperature % 10));
                        //points 25.1 = 1
                        FlashFor(_blue, (int)(temperature % 1 * 10));
                    }
                    else
                    {
                        Debug.WriteLine("DHT failed");
                        FlashLED(_red, _green);
                    }

                    FlashLED(_red, _green, _blue);

                    if (temperature > _maxTemperature)
                    {
                        _relaySwitch.Toggle();

                        FlashFor(_red, _interval);

                        _relaySwitch.Toggle();
                    }
                    else
                    {
                        FlashFor(_blue, _interval);
                    }

                }
            }
        }

        private static void FlashFor(GpioPin led, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(50);
                FlashLED(led);
            }
        }

        private static void FlashLED(params GpioPin[] leds)
        {
            foreach (var led in leds)
            {
                led.Toggle();
            }

            Thread.Sleep(500);

            foreach (var led in leds)
            {
                led.Toggle();
            }
        }
        private static void SetupGpios()
        {
            _gpio = new GpioController(PinNumberingScheme.Logical);

            _red = _gpio.OpenPin(15, PinMode.Output);
            _blue = _gpio.OpenPin(4, PinMode.Output);
            _green = _gpio.OpenPin(2, PinMode.Output);
            _relaySwitch = _gpio.OpenPin(5, PinMode.Output);
        }
    }
}
