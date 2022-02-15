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
            output = Vector.Multiply(input2, output);
            Vector<float> negatives = Vector.ConditionalSelect(Vector.Min(Vector.LessThan(input, Vector<float>.Zero), Vector.GreaterThan(input2, new Vector<float>((float)Math.PI))), Vector<float>.One, -Vector<float>.One);
            output = Vector.Multiply(output, negatives);

            return output;
        }

        public static Vector<float> Cos(Vector<float> input)
        {
            Vector<float> output;
            Vector<float> input2;

            input2 = Vector.ConditionalSelect(Vector.LessThan(input, Vector<float>.Zero), input, -input);
            input2 = Vector.ConditionalSelect(Vector.LessThan(input2, new Vector<float>((float)Math.PI)), input2, Vector.Subtract(input2, new Vector<float>((float)Math.PI)));

            Vector<float> x2 = Vector.Multiply(input2, input2);

            output = Vector.Subtract(Vector.Divide(x2, new Vector<float>(132)), Vector<float>.One);
            output = Vector.Add(Vector.Multiply(Vector.Divide(x2, new Vector<float>(90)), output), Vector<float>.One);
            output = Vector.Subtract(Vector.Multiply(Vector.Divide(x2, new Vector<float>(56)), output), Vector<float>.One);
            output = Vector.Add(Vector.Multiply(Vector.Divide(x2, new Vector<float>(30)), output), Vector<float>.One);
            output = Vector.Subtract(Vector.Multiply(Vector.Divide(x2, new Vector<float>(12)), output), Vector<float>.One);
            output = Vector.Add(Vector.Multiply(Vector.Divide(x2, new Vector<float>(2)), output), Vector<float>.One);

            Vector<float> negatives = Vector.ConditionalSelect(Vector.LessThan(input2, new Vector<float>((float)Math.PI)), Vector<float>.One, -Vector<float>.One);
            output = Vector.Multiply(output, Vector.AsVectorSingle(negatives));
            return output;
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

        public static double Cos(double x)
        {
            if (x == 0) { return 1; }
            if (x < 0) { return Cos(-x); }
            //if (x > π) { return -Cos(x - π); }
            //if (x > π4) { return Sin(π2 - x); }

            double x2 = x * x;

            return x2 / 2 * (x2 / 12 * (x2 / 30 * (x2 / 56 * (x2 / 90 * (x2 / 132 - 1) + 1) - 1) + 1) - 1) + 1;
        }
    }
}
