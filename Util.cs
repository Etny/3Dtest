using System;
using System.Collections.Generic;
using System.Text;

namespace _3Dtest
{
    class Util
    {
        public static float ToRadians(float deg) 
            => (float)((deg / 180) * Math.PI);

        public static float ToDegrees(float rad)
            => (float)(rad / Math.PI) * 180;
        
    }
}
