using System;
using Microsoft.SPOT;

namespace Ktos.MicroFramework.Extensions
{
    public static class TimespanExtension
    {
        private const long TicksPerMicroseconds = TimeSpan.TicksPerMillisecond / 1000;

        public static long TotalMiliseconds(this TimeSpan @this)
        {
            long result = @this.Ticks / TimeSpan.TicksPerMillisecond;
            return result;
        }

        public static long TotalMicroseconds(this TimeSpan @this)
        {
            long result = @this.Ticks / TicksPerMicroseconds;
            return result;
        }
    }
}
