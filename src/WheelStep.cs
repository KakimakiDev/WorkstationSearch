using System;

namespace WorkstationSearch
{
    internal static class WheelStep
    {
        internal static float FromVisibleFraction(float visible)
        {
            if (float.IsNaN(visible) || visible <= 0 || visible >= 1) return 0;
            return Math.Min(1, visible / (3 * (1 - visible)));
        }
    }
}
