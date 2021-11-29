using System;
using System.Collections.Generic;
using System.Text;

namespace CP.Procedural.Noise
{
    public class NoiseManager
    {
        public static void Test()
        {
#if NET5_0_OR_GREATER
            Console.WriteLine("I am NET 6");
#else
            Console.WriteLine("I am NET STANDARD");
#endif
        }
    }
}
