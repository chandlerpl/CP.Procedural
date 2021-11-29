using CP.Common.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPWS.Test
{
    public abstract class CountCommand : Command
    {
        public override bool Init()
        {
            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
            int samples = 10;
            args.RemoveAt(0);
            if (args.Count > 0)
            {
                samples = int.Parse(args[0]);
                args.RemoveAt(0);
            }
            int duration = 10;
            if (args.Count > 0)
            {
                duration = int.Parse(args[0]);
                args.RemoveAt(0);
            }
            int duration2 = duration * 1000;

            double[] times = new double[samples];
            Stopwatch watch = new Stopwatch();

            Console.WriteLine("Cores: " + Environment.ProcessorCount);
            Console.WriteLine("64bit:" + Environment.Is64BitProcess + Environment.NewLine);

            for (int s = 0; s < samples; s++)
            {
                PrepareTest(args.ToArray());
                int i = 0;
                watch.Restart();
                while (watch.Elapsed.TotalMilliseconds < duration2)
                {
                    i += RunTest().GetAwaiter().GetResult();
                }
                watch.Stop();

                times[s] = i / 1000000.0 / duration;
            }
            Console.WriteLine(samples + ": avg => " + times.Average() + " | best => " + times.Min() + " | worst => " + times.Max());

            return true;
        }
        public abstract void PrepareTest(params string[] args);

        public abstract Task<int> RunTest();
    }
}
