using CP.Common.Commands;
using CP.Procedural.Noise;
using System.Numerics;

CommandSystem commandSystem = new();

while (true)
{
    Console.Write("Please enter a command: ");
    commandSystem.CommandInterface(null, Console.ReadLine().Split(' '));
}