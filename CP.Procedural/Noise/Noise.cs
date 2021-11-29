using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CP.Procedural.Noise
{
    public enum FractalType { FBM, Billow, Rigid };

    public abstract class NoiseGen
    {
        protected uint Offset { get; } = 1455382547;

        private uint seed;
        private float persistence;
        private float scale;

        public virtual uint Seed { get => seed; protected set => seed = value; }
        public virtual float Persistence { get => persistence; set => persistence = value; }
        public virtual float Scale { get => scale; set => scale = value; }
        public virtual bool UseSIMD { get => false; }

        public NoiseGen(uint seed)
        {
            Seed = seed;
        }

        public abstract void Setup(int dimensions);

        public abstract float Noise(int dimensions, params float[] vals);

        public virtual Vector<float> Noise(int dimensions, params Vector<float>[] vals)
        {
            throw new NotImplementedException();
        }

        public Vector<float> FractalFBM(int iterations, int dimensions, params Vector<float>[] vals)
        {
            Vector<float> maxAmp = new Vector<float>(0);
            Vector<float> amp = new Vector<float>(1);
            Vector<float> freq = new Vector<float>(Scale);
            Vector<float> noise = new Vector<float>(0);

            Vector<float>[] nVals = new Vector<float>[vals.Length];
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    nVals[j] = vals[j] * freq;
                }

                noise += Noise(dimensions, nVals) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        public Vector<float> FractalBillow(int iterations, int dimensions, params Vector<float>[] vals)
        {
            Vector<float> maxAmp = new Vector<float>(0);
            Vector<float> amp = new Vector<float>(1);
            Vector<float> freq = new Vector<float>(Scale);
            Vector<float> noise = new Vector<float>(0);

            Vector<float>[] nVals = new Vector<float>[vals.Length];
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    nVals[j] = vals[j] * freq;
                }

                noise += (Vector.Abs(Noise(dimensions, nVals)) * 2 - new Vector<float>(1)) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        public Vector<float> FractalRigid(int iterations, int dimensions, params Vector<float>[] vals)
        {
            Vector<float> maxAmp = new Vector<float>(0);
            Vector<float> amp = new Vector<float>(1);
            Vector<float> freq = new Vector<float>(Scale);
            Vector<float> noise = new Vector<float>(0);

            Vector<float>[] nVals = new Vector<float>[vals.Length];
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    nVals[j] = vals[j] * freq;
                }

                noise += (Vector<float>.One - Vector.Abs(Noise(dimensions, nVals))) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        public float FractalFBM(int iterations, int dimensions, params float[] vals)
        {
            float maxAmp = 0;
            float amp = 1;
            float freq = Scale;
            float noise = 0;

            float[] nVals = new float[vals.Length];
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    nVals[j] = vals[j] * freq;
                }

                noise += Noise(dimensions, nVals) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        public float FractalBillow(int iterations, int dimensions, params float[] vals)
        {
            float maxAmp = 0;
            float amp = 1;
            float freq = Scale;
            float noise = 0;

            float[] nVals = new float[vals.Length];
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    nVals[j] = vals[j] * freq;
                }

                noise += (Math.Abs(Noise(dimensions, nVals)) * 2 - 1) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        public float FractalRigid(int iterations, int dimensions, params float[] vals)
        {
            float maxAmp = 0;
            float amp = 1;
            float freq = Scale;
            float noise = 0;

            float[] nVals = new float[vals.Length];
            for (int i = 0; i < iterations; ++i)
            {
                for (int j = 0; j < vals.Length; ++j)
                {
                    nVals[j] = vals[j] * freq;
                }

                noise += (1 - Math.Abs(Noise(dimensions, nVals))) * amp;
                maxAmp += amp;
                amp *= Persistence;
                freq *= 2;
            }

            noise /= maxAmp;

            return noise;
        }

        public async Task<float[,]> NoiseMap(int iterations, FractalType type, params int[] vals)
        {
            int len = vals.Length;
            int yLen = (len < 2 || vals[1] == 0) ? 1 : vals[1];
            int xLen = vals[0];
            int simdCount = Vector<float>.Count;
            Task[] tasks = new Task[UseSIMD ? yLen / simdCount : yLen];

            var buffer = new float[yLen, xLen];
            Setup(len);

            if(UseSIMD)
            {
                for (int y = 0, pos = 0; y < yLen; y += simdCount, pos++)
                {
                    int yCopy = y;
                    tasks[pos] = Task.Run(() =>
                    {
                        Vector<float>[] values = new Vector<float>[len];
                        for (int j = 2; j < vals.Length; ++j)
                        {
                            values[j] = new Vector<float>(vals[j]);
                        }
                        if (len > 1)
                        {
                            var simd = new float[simdCount];
                            for (int i = 0; i < simdCount; ++i)
                            {
                                simd[i] = yCopy + i;
                            }
                            values[1] = new Vector<float>(simd);
                        }

                        for (int x = 0; x < xLen; ++x)
                        {
                            values[0] = new Vector<float>(x);
                            Vector<float> result;
                            switch (type)
                            {
                                case FractalType.FBM:
                                    result = FractalFBM(iterations, len, values);
                                    for (int i = 0; i < simdCount; ++i)
                                    {
                                        buffer[yCopy + i, x] = result[i];
                                    }
                                    break;
                                case FractalType.Billow:
                                    result = FractalBillow(iterations, len, values);
                                    for (int i = 0; i < simdCount; ++i)
                                    {
                                        buffer[yCopy + i, x] = result[i];
                                    }
                                    break;
                                case FractalType.Rigid:
                                    result = FractalRigid(iterations, len, values);
                                    for (int i = 0; i < simdCount; ++i)
                                    {
                                        buffer[yCopy + i, x] = result[i];
                                    }
                                    break;
                            }
                        }
                    });
                }

            }
            else
            {
                for (int y = 0; y < yLen; ++y)
                {
                    int yCopy = y;
                    tasks[y] = Task.Run(() =>
                    {
                        float[] values = new float[len];
                        for (int j = 2; j < vals.Length; ++j)
                        {
                            values[j] = vals[j];
                        }
                        if (len > 1)
                            values[1] = yCopy;

                        for (int x = 0; x < xLen; ++x)
                        {
                            values[0] = x;
                            switch (type)
                            {
                                case FractalType.FBM:
                                    buffer[yCopy, x] = FractalFBM(iterations, len, values);
                                    break;
                                case FractalType.Billow:
                                    buffer[yCopy, x] = FractalBillow(iterations, len, values);
                                    break;
                                case FractalType.Rigid:
                                    buffer[yCopy, x] = FractalRigid(iterations, len, values);
                                    break;
                            }
                        }
                    });
                }

            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return buffer;
        }

        public float[,] NoiseMapNotAsync(int iterations, FractalType type, params int[] vals)
        {
            int len = vals.Length;
            int yLen = (len < 2 || vals[1] == 0) ? 1 : vals[1];
            int xLen = vals[0];
            var buffer = new float[yLen, xLen];

            Setup(len);
            if(UseSIMD)
            {
                int simdCount = Vector<float>.Count;
                Vector<float>[] values = new Vector<float>[len];
                for (int j = 2; j < vals.Length; ++j)
                {
                    values[j] = new Vector<float>(vals[j]);
                }
                var simd = new float[simdCount];
                for (int y = 0; y < yLen; y += simdCount)
                {
                    if (len > 1)
                    {
                        for(int i = 0; i < simdCount; ++i)
                        {
                            simd[i] = y + i;
                        }
                        values[1] = new Vector<float>(simd);
                    }
                    for (int x = 0; x < xLen; ++x)
                    {
                        values[0] = new Vector<float>(x);
                        Vector<float> result;
                        switch (type)
                        {
                            case FractalType.FBM:
                                result = FractalFBM(iterations, len, values);
                                for (int i = 0; i < simdCount; ++i)
                                {
                                    buffer[y + i, x] = result[i];
                                }
                                break;
                            case FractalType.Billow:
                                result = FractalBillow(iterations, len, values);
                                for (int i = 0; i < simdCount; ++i)
                                {
                                    buffer[y + i, x] = result[i];
                                }
                                break;
                            case FractalType.Rigid:
                                result = FractalRigid(iterations, len, values);
                                for (int i = 0; i < simdCount; ++i)
                                {
                                    buffer[y + i, x] = result[i];
                                }
                                break;
                        }
                    }
                }
            } else
            {
                float[] values = new float[len];
                for (int j = 2; j < vals.Length; ++j)
                {
                    values[j] = vals[j];
                }
                for (int y = 0; y < yLen; ++y)
                {
                    if (len > 1)
                        values[1] = y;
                    for (int x = 0; x < xLen; ++x)
                    {
                        values[0] = x;
                        switch (type)
                        {
                            case FractalType.FBM:
                                buffer[y, x] = FractalFBM(iterations, len, values);
                                break;
                            case FractalType.Billow:
                                buffer[y, x] = FractalBillow(iterations, len, values);
                                break;
                            case FractalType.Rigid:
                                buffer[y, x] = FractalRigid(iterations, len, values);
                                break;
                        }
                    }
                }
            }

            return buffer;
        }

        public async Task<float[]> NoiseMap(int permutations, FractalType type, float[][] vals, float strength = 1, float min = -1, float max = 1)
        {
            int dimensions = vals[0].Length;
            int simdCount = Vector<float>.Count;
            int len = vals.Length;
            Task[] tasks = new Task[UseSIMD ? (len + simdCount - 1) / simdCount : len];

            float[] buffer = new float[len];
            Setup(dimensions);

            if (UseSIMD)
            {
                int simdPosition = 0;
                int taskPosition = 0;
                float[][] values = new float[dimensions][];
                for (int i = 0; i < dimensions; i++)
                {
                    values[i] = new float[simdCount];
                }
                for (int i = 0; i < len; ++i)
                {
                    for (int k = 0; k < dimensions; k++)
                    {
                        values[k][simdPosition] = vals[i][k];
                    }
                    if (++simdPosition >= simdCount || i == len - 1)
                    {
                        simdPosition = 0;

                        Vector<float>[] vectors = new Vector<float>[dimensions];
                        for (int k = 0; k < dimensions; ++k)
                        {
                            vectors[k] = new Vector<float>(values[k]);
                        }

                        int iCopy = (i + 1) - simdCount;
                        tasks[taskPosition++] = Task.Factory.StartNew(() =>
                        {
                            Vector<float> result;
                            switch (type)
                            {
                                case FractalType.FBM:
                                    result = FractalFBM(permutations, dimensions, vectors);
                                    break;
                                case FractalType.Billow:
                                    result = FractalBillow(permutations, dimensions, vectors);
                                    break;
                                case FractalType.Rigid:
                                    result = FractalRigid(permutations, dimensions, vectors);
                                    break;
                                default:
                                    return;
                            }

                            result = Vector.Max(result, new Vector<float>(min));
                            result = Vector.Min(result, new Vector<float>(max));

                            Vector<float> str = new Vector<float>(strength);
                            result = (str + str) / new Vector<float>(2) * (result - Vector<float>.One) + str;

                            for (int j = 0; j < simdCount; ++j)
                            {
                                if (iCopy + j >= len) break;
                                buffer[iCopy + j] = result[j];
                            }
                        });
                    }
                }
            } else
            {
                for (int i = 0; i < len; ++i)
                {
                    int iCopy = i;
                    tasks[iCopy] = Task.Factory.StartNew(() =>
                    {
                        float val = buffer[iCopy];
                        switch (type)
                        {
                            case FractalType.FBM:
                                buffer[iCopy] = FractalFBM(permutations, dimensions, vals[iCopy]);
                                break;
                            case FractalType.Billow:
                                buffer[iCopy] = FractalBillow(permutations, dimensions, vals[iCopy]);
                                break;
                            case FractalType.Rigid:
                                buffer[iCopy] = FractalRigid(permutations, dimensions, vals[iCopy]);
                                break;
                            default:
                                return;
                        }
                    });
                }
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return buffer;
        }
    }
}
