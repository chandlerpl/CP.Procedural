using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CP.Procedural.Noise
{
    public partial class SimplexNoise : NoiseGen
    {
        private static readonly int[] primeList = new[] { 1619, 31337, 6971, 1013 };
        private static readonly float[] GValues = new[] { 0.292893f, 0.211324f, 0.166666f, 0.138196f };
        private static readonly float[] FValues = new[] { 0.414213f, 0.366025f, 0.333333f, 0.309016f };

        public SimplexNoise(uint seed, float scale, float persistence) : base(seed)
        {
            Scale = scale;
            Persistence = persistence;
        }

        public override void Setup(int dimensions)
        {
            if (dimensions - 1 < 4)
            {
                G = GValues[dimensions - 1];
                F = FValues[dimensions - 1];
            }
            else
            {
                float sqrt = (float)Math.Sqrt(dimensions + 1);
                G = ((dimensions + 1) - sqrt) / ((dimensions + 1) * dimensions);
                F = (sqrt - 1) / dimensions;
            }

            ivvals = new int[dimensions];
            vvals = new float[dimensions];
            xvals = new float[dimensions];
            ranks = new float[dimensions];
        }

        float G;
        float F;

        int[] ivvals;
        float[] vvals;
        float[] xvals;
        float[] ranks;
        public override float Noise(int dimensions, params float[] vals)
        {
            if (dimensions == 1)
            {
                return Noise(vals[0]);
            }
            else if (dimensions == 2)
            {
                return Noise(vals[0], vals[1]);
            }
            else if (dimensions == 3)
            {
                return Noise(vals[0], vals[1], vals[2]);
            }
            else if (dimensions == 4)
            {
                return Noise(vals[0], vals[1], vals[2], vals[3]);
            }

            float s = 0;

            foreach (float v in vals)
            {
                s += v;
            }
            s *= F;

            float t = 0;
            for (int i = 0; i < dimensions; ++i)
            {
                ivvals[i] = (int)(vals[i] + s);
                t += ivvals[i];
            }
            t *= G;

            for (int i = dimensions - 1; i >= 0; --i)
            {
                xvals[i] = vals[i] - (ivvals[i] - t);
                for (int j = i + 1; j < dimensions; ++j)
                {

                    if (xvals[i] > xvals[j])
                    {
                        ranks[i]++;
                    }
                    else
                    {
                        ranks[j]++;
                    }
                }
            }
            float n = 0;
            int temp = dimensions - 1;

            for (int i = 0; i < dimensions + 1; ++i)
            {
                t = 0.6f;
                uint hash = Seed;

                for (int j = 0; j < dimensions; ++j)
                {
                    int ival = 0;
                    if (i > 0)
                    {
                        ival = (i == dimensions ? 1 : (ranks[j] >= temp ? 1 : 0));
                    }
                    float vval = vvals[j] = i == 0 ? xvals[j] : xvals[j] - ival + i * G;

                    t -= vval * vval;

                    hash ^= (uint)(primeList[j % 4] * (ivvals[j] + ival));
                }
                if (i > 0)
                {
                    temp--;
                }
                if (t >= 0)
                {
                    hash = hash * hash * hash * 60493;
                    hash = ((hash >> 13) ^ hash) & 15;

                    float result = 0.0f;
                    int current = 1;

                    for (int j = dimensions - 1; j > -1; --j)
                    {
                        result += (hash & current) == 0 ? -vvals[j] : vvals[j];
                        current *= 2;
                    }

                    n += (t * t) * t * t * result;
                }
            }

            return 32.0f * n;
        }

        private float Noise(float x, float y, float z, float w)
        {
            float s = (x + y + z + w) * F;

            int xival = (int)(x + s);
            int yival = (int)(y + s);
            int zival = (int)(z + s);
            int wival = (int)(w + s);
            float t = (xival + yival + zival + wival) * G;

            int xrank = 0;
            int yrank = 0;
            int zrank = 0;
            int wrank = 0;
            float wxval = w - (wival - t);
            float zxval = z - (zival - t);
            if (wxval > zxval) zrank++; else wrank++;

            float yxval = y - (yival - t);
            if (yxval > zxval) yrank++; else zrank++;
            if (yxval > wxval) yrank++; else wrank++;
            float xxval = x - (xival - t);
            if (xxval > yxval) xrank++; else yrank++;
            if (xxval > zxval) xrank++; else zrank++;
            if (xxval > wxval) xrank++; else wrank++;

            float n = 0;

            // Round 0
            t = 0.6f;
            uint hash = Seed;
            hash ^= (uint)(1619 * xival) ^ (uint)(31337 * yival) ^ (uint)(6971 * zival) ^ (uint)(1013 * wival);
            t -= (xxval * xxval) - (yxval * yxval) - (zxval * zxval) -(wxval * wxval);
            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -wxval : wxval + (hash & 2) == 0 ? -zxval : zxval + (hash & 4) == 0 ? -yxval : yxval + (hash & 8) == 0 ? -xxval : xxval);
            }

            // Round 1
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * (xival * (xrank >= 2 ? 1 : 0))) ^ (uint)(31337 * (yival * (yrank >= 2 ? 1 : 0))) ^ (uint)(6971 * (zival * (zrank >= 2 ? 1 : 0))) ^ (uint)(1013 * (wival * (wrank >= 2 ? 1 : 0)));
            float xval = xxval - (xrank >= 3 ? 1 : 0) + 1 * G;
            float yval = yxval - (yrank >= 3 ? 1 : 0) + 1 * G;
            float zval = zxval - (zrank >= 3 ? 1 : 0) + 1 * G;
            float wval = wxval - (wrank >= 3 ? 1 : 0) + 1 * G;
            t -= (xval * xval) - (yval * yval) - (zval * zval) - (wval * wval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -wxval : wxval + (hash & 2) == 0 ? -zxval : zxval + (hash & 4) == 0 ? -yxval : yxval + (hash & 8) == 0 ? -xxval : xxval);
            }

            // Round 2
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * (xival * (xrank >= 1 ? 1 : 0))) ^ (uint)(31337 * (yival * (yrank >= 1 ? 1 : 0))) ^ (uint)(6971 * (zival * (zrank >= 1 ? 1 : 0))) ^ (uint)(1013 * (wival * (wrank >= 2 ? 1 : 0)));
            xval = xxval - (xrank >= 1 ? 2 : 0) + 2 * G;
            yval = yxval - (yrank >= 1 ? 2 : 0) + 2 * G;
            zval = zxval - (zrank >= 1 ? 2 : 0) + 2 * G;
            wval = wxval - (wrank >= 1 ? 2 : 0) + 2 * G;
            t -= (xval * xval) - (yval * yval) - (zval * zval) - (wval * wval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -wxval : wxval + (hash & 2) == 0 ? -zxval : zxval + (hash & 4) == 0 ? -yxval : yxval + (hash & 8) == 0 ? -xxval : xxval);
            }

            // Round 3
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * (xival * (xrank >= 1 ? 1 : 0))) ^ (uint)(31337 * (yival * (yrank >= 1 ? 1 : 0))) ^ (uint)(6971 * (zival * (zrank >= 1 ? 1 : 0))) ^ (uint)(1013 * (wival * (wrank >= 2 ? 1 : 0)));
            xval = xxval - (xrank >= 1 ? 1 : 0) + 3 * G;
            yval = yxval - (yrank >= 1 ? 1 : 0) + 3 * G;
            zval = zxval - (zrank >= 1 ? 1 : 0) + 3 * G;
            wval = wxval - (wrank >= 1 ? 1 : 0) + 3 * G;
            t -= (xval * xval) - (yval * yval) - (zval * zval) - (wval * wval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -wxval : wxval + (hash & 2) == 0 ? -zxval : zxval + (hash & 4) == 0 ? -yxval : yxval + (hash & 8) == 0 ? -xxval : xxval);
            }

            // Round 4
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * xival) ^ (uint)(31337 * yival) ^ (uint)(6971 * zival) ^ (uint)(1013 * wival);
            xval = xxval - 1 + 4 * G;
            yval = yxval - 1 + 4 * G;
            zval = zxval - 1 + 4 * G;
            wval = wxval - 1 + 4 * G;
            t -= (xval * xval) - (yval * yval) - (zval * zval) - (wval * wval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -wxval : wxval + (hash & 2) == 0 ? -zxval : zxval + (hash & 4) == 0 ? -yxval : yxval + (hash & 8) == 0 ? -xxval : xxval);
            }

            return 32.0f * n;
        }

        public float Noise(float x, float y, float z)
        {
            float s = (x + y + z) * F;

            int xival = (int)(x + s);
            int yival = (int)(y + s);
            int zival = (int)(z + s);
            float t = (xival + yival + zival) * G;

            float zxval = z - (zival + t);
            float yxval = y - (yival + t);
            int xrank = 0;
            int yrank = 0;
            int zrank = 0;
            if (yxval > zxval) yrank++; else zrank++;

            float xxval = x - (xival + t);
            if (xxval > yxval) xrank++; else yrank++;
            if (xxval > zxval) xrank++; else zrank++;

            float n = 0;

            // Round 0
            t = 0.6f;
            uint hash = Seed;
            hash ^= (uint)(1619 * xival) ^ (uint)(31337 * yival) ^ (uint)(6971 * zival);
            t -= (xxval * xxval) - (yxval * yxval) - (zxval * zxval);
            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -zxval : zxval + (hash & 2) == 0 ? -yxval : yxval + (hash & 4) == 0 ? -xxval : xxval);
            }

            // Round 1
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * (xival * (xrank >= 2 ? 1 : 0))) ^ (uint)(31337 * (yival * (yrank >= 2 ? 1 : 0))) ^ (uint)(6971 * (zival * (zrank >= 2 ? 1 : 0)));
            float xval = xxval - (xrank >= 2 ? 1 : 0) + 1 * G;
            float yval = yxval - (yrank >= 2 ? 1 : 0) + 1 * G;
            float zval = zxval - (zrank >= 2 ? 1 : 0) + 1 * G;
            t -= (xval * xval - yval * yval - zval * zval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -zval : zval + (hash & 2) == 0 ? -yval : yval + (hash & 4) == 0 ? -xval : xval);
            }

            // Round 2
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * (xival * (xrank >= 1 ? 1 : 0))) ^ (uint)(31337 * (yival * (yrank >= 1 ? 1 : 0))) ^ (uint)(6971 * (zival * (zrank >= 1 ? 1 : 0)));
            xval = xxval - (xrank >= 1 ? 1 : 0) + 2 * G;
            yval = yxval - (yrank >= 1 ? 1 : 0) + 2 * G;
            zval = zxval - (zrank >= 1 ? 1 : 0) + 2 * G;
            t -= (xval * xval - yval * yval - zval * zval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -zval : zval + (hash & 2) == 0 ? -yval : yval + (hash & 4) == 0 ? -xval : xval);
            }

            // Round 3
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * xival) ^ (uint)(31337 * yival) ^ (uint)(6971 * zival);
            xval = xxval - 1 + 3 * G;
            yval = yxval - 1 + 3 * G;
            zval = zxval - 1 + 3 * G;
            t -= (xval * xval - yval * yval - zval * zval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -zval : zval + (hash & 2) == 0 ? -yval : yval + (hash & 4) == 0 ? -xval : xval);
            }

            return 32.0f * n;
        }

        private float Noise(float x, float y)
        {
            float s = (x + y) * F;

            int xival = (int)(x + s);
            int yival = (int)(y + s);
            float t = (xival + yival) * G;

            float yxval = y - (yival - t);
            int xrank = 0;
            int yrank = 0;
            float xxval = x - (xival - t);
            if (xxval > yxval) xrank++; else yrank++;

            float n = 0;

            // Round 0
            t = 0.6f;
            uint hash = Seed;
            hash ^= (uint)(1619 * xival) ^ (uint)(31337 * yival);
            t -= (xxval * xxval) - (yxval * yxval);
            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -yxval : yxval + (hash & 2) == 0 ? -xxval : xxval);
            }

            // Round 1
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * (xival * (xrank >= 1 ? 1 : 0))) ^ (uint)(31337 * (yival * (yrank >= 1 ? 1 : 0)));
            float xval = xxval - (xrank >= 1 ? 1 : 0) + 1 * G;
            float yval = yxval - (yrank >= 1 ? 1 : 0) + 1 * G;
            t -= (xval * xval) - (yval * yval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -yxval : yxval + (hash & 2) == 0 ? -xxval : xxval);
            }

            // Round 2
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * xival) ^ (uint)(31337 * yival);
            xval = xxval - 1 + 2 * G;
            yval = yxval - 1 + 2 * G;
            t -= (xval * xval) - (yval * yval);

            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -yxval : yxval + (hash & 2) == 0 ? -xxval : xxval);
            }

            return 32.0f * n;
        }

        private float Noise(float x)
        {
            float s = x * F;

            int xival = (int)(x + s);
            float t = xival * G;

            float xxval = x - (xival - t);

            float n = 0;

            // Round 0
            t = 0.6f;
            uint hash = Seed;
            hash ^= (uint)(1619 * xival);
            t -= (xxval * xxval);
            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -xxval : xxval);
            }

            // Round 2
            t = 0.6f;
            hash = Seed;
            hash ^= (uint)(1619 * xival);
            float xval = xxval - 1 + 1 * G;
            t -= (xval * xval);
            if (t >= 0)
            {
                hash = hash * hash * hash * 60493;
                hash = ((hash >> 13) ^ hash) & 15;
                n += (t * t) * t * t * ((hash & 1) == 0 ? -xxval : xxval);
            }

            return 32.0f * n;
        }


    }
}
