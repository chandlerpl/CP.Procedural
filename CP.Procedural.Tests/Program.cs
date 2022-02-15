using CP.Common.Commands;
using CP.Procedural.Maths;
using CP.Procedural.Noise;
using System.Numerics;

CommandSystem commandSystem = new();

while (true)
{
    Console.WriteLine(VectorTrig.Sin(Vector<float>.One));
    Console.WriteLine(VectorTrig.Sin(1f));
    Console.Write("Please enter a command: ");
    commandSystem.CommandInterface(null, Console.ReadLine().Split(' '));
}