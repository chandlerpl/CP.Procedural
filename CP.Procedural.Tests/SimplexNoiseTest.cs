using CP.Procedural.Noise;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CPWS.Test
{
    class SimplexNoiseTest : TimedCommand
    {
        public override bool Init()
        {
            Name = "Simplex";
            Desc = "Generates a Simplex Noise";
            Aliases = new List<string>() { "simplex" };
            ProperUse = "simplex [samples (default: 10)] [permutations (default: 4)] [dimension values (default: { 1920, 1080, 0 })]";

            return true;
        }

        SimplexNoise noise;
        int permutations = -1;
        int[] dims;
        float[][] vals;
        public override void PrepareTest(params string[] args)
        {
            Random rand = new Random();
            noise = new SimplexNoise((uint)rand.Next(0, 999999999), 0.5f, 0.005f);

            if(vals == null)
            {
                vals = new float[10][];
                for (int i = 0; i < 10; i++)
                {
                    vals[i] = new float[3];
                    vals[i][0] = rand.NextSingle() * 4096;
                    vals[i][1] = rand.NextSingle() * 4096;
                    vals[i][2] = rand.NextSingle() * 4096;
                }
            }

            if (permutations == -1)
            {
                permutations = 4;
                if (args.Length > 0)
                {
                    permutations = int.Parse(args[0]);
                }

                dims = args.Length > 1 ? new int[args.Length - 2] : new int[] { 1920, 1080, 0 };
                int j = 0;
                if (args.Length > 1)
                    for (int i = 3; i < args.Length; i++)
                        dims[j++] = int.Parse(args[i]);
            }
        }

        public override async Task RunTest()
        {
            //_ = noise.NoiseMapNotAsync(permutations, FractalType.FBM, dims);
            float[] results = await noise.NoiseMap(permutations, FractalType.FBM, vals);

            for(int i = 0; i < 10; i++)
            {
                Console.WriteLine(i + ": " + vals[i][0] + ", " + vals[i][1] + ", " + vals[i][2] + " = " + results[i]);
            }

            //return 1920 * 1080;
        }
    }
}
