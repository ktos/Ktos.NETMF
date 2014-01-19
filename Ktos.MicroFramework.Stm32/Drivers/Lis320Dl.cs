#region License
/*
 * Copyright 2012 lawrence_jeff
 * Copyright 2014 Marcin "Ktos" Badurowicz <m at badurowicz dot net>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at

 * http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace Ktos.MicroFramework.Stm32.Drivers
{
    /// <summary>
    /// Represents a Single LIS302DL Accelerometer on a SPI Bus
    /// 
    /// This class was originally made by lawrence_jeff in 2012, and than
    /// was slightly modified.
    /// 
    /// Original avaliable at:
    /// https://www.ghielectronics.com/community/codeshare/entry/436    
    /// </summary>
    public class Lis302Dl
    {
        /// <summary>
        /// Enum defining supported data rates for LIS302DL
        /// </summary>
        public enum DataRate
        {
            Rate100Hz = 0,
            Rate400Hz = 1
        }

        /// <summary>
        /// Enum defining power down modes
        /// </summary>
        public enum PowerDown
        {
            Down = 0,
            Active = 1
        }

        /// <summary>
        /// Enum defining supported data scales
        /// </summary>
        public enum Scale
        {
            Scale2g = 0,
            Scale9g = 1
        }

        public class Configuration
        {
            public DataRate DataRate { get; set; }
            public PowerDown PowerDown { get; set; }
            public Scale Scale { get; set; }
            public bool SelfTest { get; set; }
            public bool EnableX { get; set; }
            public bool EnableY { get; set; }
            public bool EnableZ { get; set; }

            public byte ToRegisterConfig()
            {
                int result = 0;
                result |= ((int)this.DataRate << 7) | ((int)this.PowerDown << 6) | ((int)this.Scale << 5);

                if (this.SelfTest)
                    result |= 0x18; // 0b11000

                if (this.EnableX)
                    result |= 1;

                if (this.EnableY)
                    result |= 0x2;

                if (this.EnableZ)
                    result |= 0x4;

                return (byte)result;
            }
        }

        /// <summary>
        /// Const Names consistent with STM Example Code
        /// </summary>
        #region Registers

        private const byte WHO_AM_I_ADDR = 0x0F;
        private const byte CTRL_REG1_ADDR = 0x20;
        private const byte CTRL_REG2_ADDR = 0x21;
        private const byte CTRL_REG3_ADDR = 0x22;
        private const byte HP_FILTER_RESET_REG_ADDR = 0x23;
        private const byte STATUS_REG_ADDR = 0x27;
        private const byte OUT_X_ADDR = 0x29;
        private const byte OUT_Y_ADDR = 0x2B;
        private const byte OUT_Z_ADDR = 0x2D;
        private const byte FF_WU_CFG1_REG_ADDR = 0x30;
        private const byte FF_WU_SRC1_REG_ADDR = 0x31;
        private const byte FF_WU_THS1_REG_ADDR = 0x32;
        private const byte FF_WU_DURATION1_REG_ADDR = 0x33;
        private const byte FF_WU_CFG2_REG_ADDR = 0x34;
        private const byte FF_WU_SRC2_REG_ADDR = 0x35;
        private const byte FF_WU_THS2_REG_ADDR = 0x36;
        private const byte FF_WU_DURATION2_REG_ADDR = 0x37;
        private const byte CLICK_CFG_REG_ADDR = 0x38;
        private const byte CLICK_SRC_REG_ADDR = 0x39;
        private const byte CLICK_THSY_X_REG_ADDR = 0x3B;
        private const byte CLICK_THSZ_REG_ADDR = 0x3C;
        private const byte CLICK_TIMELIMIT_REG_ADDR = 0x3D;
        private const byte CLICK_LATENCY_REG_ADDR = 0x3E;
        private const byte CLICK_WINDOW_REG_ADDR = 0x3F;

        #endregion

        /// <summary>
        /// Defines if debug messages should be printed
        /// </summary>
#if DEBUG
        bool DebugMode = true;
#else
        bool DebugMode = false;
#endif

        /// <summary>
        /// Private SPI bus instance
        /// </summary>
        private SPI _spi = null;

        private Lis302Dl.Configuration config;

        /// <summary>
        /// Creates a new instance of Lis302Dl class with standard configuration (400Hz, Active Power, 2g, not test, all axis enabled)
        /// </summary>
        /// <param name="csPin">CS pin for SPI interface</param>        
        /// <param name="config">LIS302DL configuration</param>
        public Lis302Dl(Cpu.Pin csPin)
        {
            // The 302DL is a mode 3 device
            SPI.Configuration spiConfig = new SPI.Configuration(csPin, false, 0, 0, true, true, 10000, SPI.SPI_module.SPI1);
            _spi = new SPI(spiConfig);

            this.config = new Configuration() { DataRate = DataRate.Rate400Hz, PowerDown = PowerDown.Active, Scale = Scale.Scale2g, SelfTest = false, EnableX = true, EnableZ = true, EnableY = true };
            Init();
        }

        /// <summary>
        /// Creates a new instance of Lis302Dl class
        /// </summary>
        /// <param name="csPin">CS pin for SPI interface</param>        
        /// <param name="config">LIS302DL configuration</param>
        public Lis302Dl(Cpu.Pin csPin, Lis302Dl.Configuration config)
        {
            // The 302DL is a mode 3 device
            SPI.Configuration spiConfig = new SPI.Configuration(csPin, false, 0, 0, true, true, 10000, SPI.SPI_module.SPI1);
            _spi = new SPI(spiConfig);

            this.config = config;
            Init();
        }

        /// <summary>
        /// Creates a new instance of Lis302Dl class
        /// </summary>
        /// <param name="csPin">CS pin for SPI interface</param>
        /// <param name="spiModule">SPI module</param>
        /// /// <param name="config">LIS302DL configuration</param>
        public Lis302Dl(Cpu.Pin csPin, SPI.SPI_module spiModule, Lis302Dl.Configuration config)
        {
            SPI.Configuration spiConfig = new SPI.Configuration(csPin, false, 0, 0, true, true, 10000, spiModule);
            _spi = new SPI(spiConfig);

            this.config = config;
            Init();
        }

        /// <summary>
        /// Creates a new instance of Lis302Dl class
        /// </summary>
        /// <param name="csPin">CS pin for SPI interface</param>
        /// <param name="spiModule">SPI module</param>
        /// <param name="Clock_Rate_KHZ">SPI clock rate (defaults to 1 MHz in other constructors)</param>
        /// <param name="config">LIS302DL configuration</param>
        public Lis302Dl(Cpu.Pin csPin, SPI.SPI_module spiModule, uint Clock_Rate_KHZ, Lis302Dl.Configuration config)
        {
            SPI.Configuration spiConfig = new SPI.Configuration(csPin, false, 0, 0, true, true, Clock_Rate_KHZ, spiModule);
            _spi = new SPI(spiConfig);

            this.config = config;
            Init();
        }

        /// <summary>
        /// Writes a message to debug output, but only if DebugMode is true
        /// </summary>
        /// <param name="message">A message to be printed on debug output</param>
        private void debugMessage(string message)
        {
            if (this.DebugMode)
            {
                Debug.Print(message);
            }
        }

        /// <summary>
        /// Device initialization
        /// </summary>
        private void Init()
        {
            debugMessage("Starting Init");

            // Check for the device - not requiredm but a good practice
            if (ReadRegister(WHO_AM_I_ADDR) == 0x3b) // the proper response if the device is available is 3b (It does not need to be enabled for this to work)
            {
                debugMessage("Device Responded correctly");
            }
            else
            {
                debugMessage("Device did NOT respond to WHOAMI query correctly");

                throw new Exception("Device did not respond correctly to WHOAMI query");
            }

            ConfigureDevice(this.config);

            // FeeFallFilter and other options can be set here using Reg21 see Datasheet
            // WriteRegister(CTRL_REG2_ADDR, 0xC7);
        }

        /// <summary>
        /// Sends new configuration to device
        /// </summary>
        /// <param name="config"></param>
        public void ConfigureDevice(Configuration config)
        {
            // configures chip with configuration out of Lis302Dl.Configuration class converted to suitable byte
            WriteRegister(CTRL_REG1_ADDR, config.ToRegisterConfig());

            // Upon init you need to add some time for it to wake up
            debugMessage("Pausing for device enable");

            Thread.Sleep(10); // Allow time to startup
        }

        /// <summary>
        /// Reads data from a register
        /// </summary>
        /// <param name="register">An address of a register</param>
        /// <returns>Data in a register</returns>
        private byte ReadRegister(byte register)
        {
            // Reads and writes take 16 clock pulses so write 2 bytes and then read
            byte[] tx_data = new byte[2];
            byte[] rx_data = new byte[2];
            tx_data[0] = (byte)(register | 0x80); // MSB needs to be 1 for Read (so OR the register address with hex 80 which is 10000000)
            tx_data[1] = 0; // You have to write 2 bytes to get the device to respond - so in byte 2 just write 0
            _spi.WriteRead(tx_data, rx_data);
            return rx_data[1];
        }

        /// <summary>
        /// Writes data to a device register
        /// </summary>
        /// <param name="register">Register address</param>
        /// <param name="data">A data to be written</param>
        private void WriteRegister(byte register, byte data)
        {
            // Reads and writes take 16 clock pulses so write 2 bytes and then read
            byte[] tx_data = new byte[2];
            byte[] rx_data = new byte[2];
            tx_data[0] = (byte)(register | 0x00); // MSB needs to be 0 for Write (so or with 00000000) - This isn't needed but is helpful for code/learning
            tx_data[1] = data;
            _spi.Write(tx_data); // Used Write here and Not WriteRead because the device does not respond when you change a register value
        }

        /// <summary>
        /// Returns acceleration for X axis (in gs)
        /// </summary>
        public double ValueX
        {
            get
            {
                return calculateGs(ReadRegister(OUT_X_ADDR));
            }
        }

        /// <summary>
        /// Returns acceleration for Y axis (in gs)
        /// </summary>
        public double ValueY
        {
            get
            {
                return calculateGs(ReadRegister(OUT_Y_ADDR));
            }
        }

        /// <summary>
        /// Returns acceleration for Z axis (in gs)
        /// </summary>
        public double ValueZ
        {
            get
            {
                return calculateGs(ReadRegister(OUT_Z_ADDR));
            }
        }

        private const double RANGE2G = 4.6f;
        private const double RANGE9G = 18.4f;

        /// <summary>
        /// Calculates reading value in gs based on pure 1-byte result
        /// </summary>
        /// <param name="reading"></param>
        /// <returns></returns>
        private double calculateGs(byte reading)
        {
            int s = this.shiftReading(reading);
            double r;

            if (this.config.Scale == Scale.Scale2g)
                r = (double)s / 256 * RANGE2G;
            else
                r = (double)s / 256 * RANGE9G;

            return r;
        }

        /// <summary>
        /// Shifts internal 0-255 readings to values from -128 to 127.
        /// </summary>
        /// <param name="reading">Internal reading value</param>
        /// <returns>Value with shift if needed</returns>
        private int shiftReading(byte reading)
        {
            int result = reading;

            if (reading > 128)
                result = (256 - reading) * -1;

            return result;
        }
    }
}
