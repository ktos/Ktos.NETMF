using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Ktos.MicroFramework.Drivers
{
    /// <summary>
    /// Class driver for MMA7361 analog accelerometer
    /// </summary>
    public class Mma7361
    {
        /// <summary>
        /// Sensitivity toggle, allows values 1.5g or 6g
        /// </summary>
        public enum ScaleMode
        {
            /// <summary>
            /// 1.5g sensitivity (800 mV/g)
            /// </summary>
            Scale1p5g,

            /// <summary>
            /// 6g sensitivity (206 mV/g)
            /// </summary>
            Scale6g
        }

        /// <summary>
        /// Port for triggering accelerometer on or off
        /// </summary>
        private OutputPort sleep;

        /// <summary>
        /// Port for setting sensitivity
        /// </summary>
        private OutputPort sensitivity;

        /// <summary>
        /// Port for reading X value
        /// </summary>
        private AnalogInput x;

        /// <summary>
        /// Port for reading Y value
        /// </summary>
        private AnalogInput y;

        /// <summary>
        /// Port for reading Z value
        /// </summary>
        private AnalogInput z;
        
        /// <summary>
        /// Correction value for X
        /// </summary>
        private float xc;

        /// <summary>
        /// Correction value for Y
        /// </summary>
        private float yc;

        /// <summary>
        /// Correction value for Z
        /// </summary>
        private float zc;

        /// <summary>
        /// Maximum reading value, calculated from AnalogInput
        /// </summary>
        private float maxv;

        /// <summary>
        /// Gets values of acceleration in X axis
        /// </summary>
        public float X
        {
            get
            {
                float res = (this.Scale == ScaleMode.Scale1p5g) ? 1.5f : 6f;
                float result = this.map(this.x.ReadRaw(), 0, 4096, -res, res) - this.xc;
                if (result > res) result = res - result;

                return result;
            }
        }

        /// <summary>
        /// Gets values of acceleration in Y axis
        /// </summary>
        public float Y
        {
            get
            {
                float res = (this.Scale == ScaleMode.Scale1p5g) ? 1.5f : 6f;
                float result = this.map(this.y.ReadRaw(), 0, 4096, -res, res) - this.yc;
                if (result > res) result = res - result;

                return result;
            }
        }

        /// <summary>
        /// Gets values of acceleration in Z axis
        /// </summary>
        public float Z
        {
            get
            {
                float res = (this.Scale == ScaleMode.Scale1p5g) ? 1.5f : 6f;
                float result = this.map(this.z.ReadRaw(), 0, 4096, -res, res) - this.zc;
                if (result > res) result = res - result;

                return result;
            }
        }

        /// <summary>
        /// Private member for setting sensitivity
        /// </summary>
        private ScaleMode scale;

        /// <summary>
        /// Gets or sets device sensitivity
        /// </summary>
        public ScaleMode Scale
        {
            get
            {
                return this.scale;
            }

            set
            {
                if (value != this.scale)
                {
                    this.sensitivity.Write(!(this.Scale == ScaleMode.Scale1p5g));
                    this.scale = value;
                }                
            }
        }

        /// <summary>
        /// Creates a new instance of MMA7361 driver
        /// </summary>
        /// <param name="sleepPin">Pin for triggering device on or off</param>
        /// <param name="selfTestPin">Pin for self-test mode (not used)</param>
        /// <param name="zeroGPin">Pin for 0G detection (not used)</param>
        /// <param name="sensitivityPin">Pin for selecting sensitivity</param>
        /// <param name="valueX">Analog pin for X value</param>
        /// <param name="valueY">Analog pin for Y value</param>
        /// <param name="valueZ">Analog pin for Z value</param>
        public Mma7361(Cpu.Pin sleepPin, Cpu.Pin selfTestPin, Cpu.Pin zeroGPin, Cpu.Pin sensitivityPin, Cpu.AnalogChannel valueX, Cpu.AnalogChannel valueY, Cpu.AnalogChannel valueZ)
        {
            this.sleep = new OutputPort(sleepPin, false);
            this.sensitivity = new OutputPort(sensitivityPin, false);
            this.x = new AnalogInput(valueX);
            this.y = new AnalogInput(valueY);
            this.z = new AnalogInput(valueZ);

            this.Scale = Mma7361.ScaleMode.Scale1p5g;
            this.maxv = (float)System.Math.Pow(2, this.x.Precision);
        }

        /// <summary>
        /// Starts the device, turns it on
        /// </summary>
        public void Start()
        {
            this.sleep.Write(true);
        }

        /// <summary>
        /// Turns device off
        /// </summary>
        public void Stop()
        {
            this.sleep.Write(false);
        }

        /// <summary>
        /// A very basic calibration
        /// <para>Ten measurements in a period of 2 seconds are written and the average value is being substracted from the readings. Device MUST BE flat on a surface to test!</para>
        /// </summary>
        public void Calibrate()
        {
            float res = (this.Scale == ScaleMode.Scale1p5g) ? 1.5f : 6f;

            for (int i = 0; i < 10; i++)
            {
                var dx = this.map(this.x.ReadRaw(), 0, this.maxv, -res, res);
                var dy = this.map(this.y.ReadRaw(), 0, this.maxv, -res, res);
                var dz = this.map(this.z.ReadRaw(), 0, this.maxv, -res, res);

                this.xc += dx;
                this.yc += dy;
                this.zc += dz;

                Thread.Sleep(200);
            }

            this.xc = this.xc / 10;
            this.yc = this.yc / 10;
            this.zc = (this.zc / 10) - 1;
        }

        /// <summary>
        /// Mapping value from one range to another range
        /// </summary>
        /// <param name="d">Value to be converted</param>
        /// <param name="fromLow">Low boundary of range from it will be mapped</param>
        /// <param name="fromHigh">High boundary of first range</param>
        /// <param name="toLow">Low boundary of range to which will be mapped</param>
        /// <param name="toHigh">High boundary of the second range</param>
        /// <returns>Returns mapped value from the "from" range to "to" range</returns>
        private float map(float d, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return toLow + ((d - fromLow) * (toHigh - toLow) / (fromHigh - fromLow));
        }
    }
}
