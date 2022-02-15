using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CP.Procedural.Maths
{
    public partial class VectorTrig
    {
        public static Vector<float> Sin(Vector<float> input)
        {
            Vector<float> output;
            Vector<float> input2;
            input2 = Vector.ConditionalSelect(Vector.LessThan(input, Vector<float>.Zero), input, -input);
            input2 = Vector.ConditionalSelect(Vector.LessThan(input2, new Vector<float>((float)Math.PI)), input2, Vector.Subtract(input2, new Vector<float>((float)Math.PI)));

            Vector<float> x2 = Vector.Multiply(input2, input2);

            output = Vector.Subtract(Vector.Divide(x2, new Vector<float>(156)), Vector<float>.One);
            output = Vector.Add(Vector.Multiply(Vector.Divide(x2, new Vector<float>(110)), output), Vector<float>.One);
            output = Vector.Subtract(Vector.Multiply(Vector.Divide(x2, new Vector<float>(72)), output), Vector<float>.One);
            output = Vector.Add(Vector.Multiply(Vector.Divide(x2, new Vector<float>(42)), output), Vector<float>.One);
            output = Vector.Subtract(Vector.Multiply(Vector.Divide(x2, new Vector<float>(20)), output), Vector<float>.One);
            output = Vector.Add(Vector.Multiply(Vector.Divide(x2, new Vector<float>(6)), output), Vector<float>.One);

            return Vector.Multiply(input2, output);
        }

        public static double Sin(double x)
        {

            if (x == 0) { return 0; }
            if (x < 0) { return -Sin(-x); }
            //if (x > π) { return -Sin(x - π); }
            //if (x > π4) { return Cos(π2 - x); }

            double x2 = x * x;

            return x * (x2 / 6 * (x2 / 20 * (x2 / 42 * (x2 / 72 * (x2 / 110 * (x2 / 156 - 1) + 1) - 1) + 1) - 1) + 1);
        }
    }
}
