using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CP.Procedural.Noise
{
    public partial class SimplexNoise : NoiseGen
    {
        private static readonly Vector<float> G1 = new Vector<float>(0.292893f);
        private static readonly Vector<float> F1 = new Vector<float>(0.414213f);
        private static readonly Vector<float> G2 = new Vector<float>(0.211324f);
        private static readonly Vector<float> F2 = new Vector<float>(0.366025f);
        private static readonly Vector<float> G3 = new Vector<float>(0.166666f);
        private static readonly Vector<float> F3 = new Vector<float>(0.333333f);
        private static readonly Vector<float> G4 = new Vector<float>(0.138196f);
        private static readonly Vector<float> F4 = new Vector<float>(0.309016f);

        public override bool UseSIMD { get => true; }

        public override Vector<float> Noise(int dimensions, params Vector<float>[] vals)
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

            throw new NotImplementedException();
            //return 32.0f;
        }

        protected Vector<float> Noise(Vector<float> xin)
        {
            Vector<float> s = xin * F1;
            Vector<int> i = FastFloor(xin + s);
            Vector<float> i0 = Vector.ConvertToSingle(i);
            Vector<float> t = i0 * G1;
            Vector<float> x0 = xin - (i0 - t);

            // Round 1
            t = new Vector<float>(0.6f) - (x0 * x0);
            Vector<float> n = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i, x0, t), Vector<float>.Zero);

            // Round 2
            Vector<float> g3 = 3.0f * G1;
            Vector<float> x1 = x0 - Vector<float>.One + g3;

            t = new Vector<float>(0.6f) - (x1 * x1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + Vector<int>.One, x1, t), Vector<float>.Zero);

            return n * 32.0f;
        }

        private Vector<float> Grad(Vector<int> i, Vector<float> x, Vector<float> t)
        {
            Vector<uint> hash = new Vector<uint>(Seed);
            hash ^= (Vector<uint>)(1619 * i);

            hash = hash * hash * hash * 60493;
            hash = (hash / new Vector<uint>(32768)) ^ hash;

            t *= t;
            return t * t * (Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & Vector<uint>.One), Vector<int>.Zero), -x, x));
        }

        protected Vector<float> Noise(Vector<float> xin, Vector<float> yin)
        {
            Vector<float> s = (xin + yin) * F2;
            Vector<int> i = FastFloor(xin + s);
            Vector<int> j = FastFloor(yin + s);
            Vector<float> i0 = Vector.ConvertToSingle(i);
            Vector<float> j0 = Vector.ConvertToSingle(j);
            Vector<float> t = (i0 + j0) * G2;
            Vector<float> x0 = xin - (i0 - t);
            Vector<float> y0 = yin - (j0 - t);

            Vector<int> xranks = Vector.GreaterThan(x0, y0);
            Vector<int> yranks = Vector.LessThanOrEqual(x0, y0);

            // Round 1
            t = new Vector<float>(0.6f) - (x0 * x0) - (y0 * y0);
            Vector<float> n = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i, j, x0, y0, t), Vector<float>.Zero);

            // Round 2
            Vector<int> i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, new Vector<int>(-1)), Vector<int>.One, Vector<int>.Zero);
            Vector<int> j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, new Vector<int>(-1)), Vector<int>.One, Vector<int>.Zero);
            Vector<float> x1 = x0 - Vector.ConvertToSingle(i1) + G2;
            Vector<float> y1 = y0 - Vector.ConvertToSingle(j1) + G2;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + i1, j + j1, x1, y1, t), Vector<float>.Zero);

            // Round 4
            Vector<float> g3 = 3.0f * G2;
            x1 = x0 - Vector<float>.One + g3;
            y1 = y0 - Vector<float>.One + g3;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + Vector<int>.One, j + Vector<int>.One, x1, y1, t), Vector<float>.Zero);

            return n * 32.0f;
        }

        private Vector<float> Grad(Vector<int> i, Vector<int> j, Vector<float> x, Vector<float> y, Vector<float> t)
        {
            Vector<uint> hash = new Vector<uint>(Seed);
            hash ^= (Vector<uint>)(1619 * i);
            hash ^= (Vector<uint>)(31337 * j);

            hash = hash * hash * hash * 60493;
            hash = (hash / new Vector<uint>(32768)) ^ hash;

            t *= t;
            return t * t * (Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & Vector<uint>.One), Vector<int>.Zero), -y, y) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(2)), Vector<int>.Zero), -x, x));
        }

        protected Vector<float> Noise(Vector<float> xin, Vector<float> yin, Vector<float> zin)
        {
            Vector<float> s = (xin + yin + zin) * F3;
            Vector<int> i = FastFloor(xin + s);
            Vector<int> j = FastFloor(yin + s);
            Vector<int> k = FastFloor(zin + s);
            Vector<float> i0 = Vector.ConvertToSingle(i);
            Vector<float> j0 = Vector.ConvertToSingle(j);
            Vector<float> k0 = Vector.ConvertToSingle(k);
            Vector<float> t = (i0 + j0 + k0) * G3;
            Vector<float> x0 = xin - (i0 - t);
            Vector<float> y0 = yin - (j0 - t);
            Vector<float> z0 = zin - (k0 - t);

            Vector<int> xranks = Vector.GreaterThan(x0, y0) + Vector.GreaterThan(x0, z0);
            Vector<int> yranks = Vector.GreaterThan(y0, z0) + Vector.LessThanOrEqual(x0, y0);
            Vector<int> zranks = Vector.LessThanOrEqual(y0, z0) + Vector.LessThanOrEqual(x0, z0);

            // Round 1
            t = new Vector<float>(0.6f) - (x0 * x0) - (y0 * y0) - (z0 * z0);
            Vector<float> n = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i, j, k, x0, y0, z0, t), Vector<float>.Zero);

            // Round 2
            Vector<int> i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, new Vector<int>(-2)), Vector<int>.One, Vector<int>.Zero);
            Vector<int> j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, new Vector<int>(-2)), Vector<int>.One, Vector<int>.Zero);
            Vector<int> k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, new Vector<int>(-2)), Vector<int>.One, Vector<int>.Zero);
            Vector<float> x1 = x0 - Vector.ConvertToSingle(i1) + G3;
            Vector<float> y1 = y0 - Vector.ConvertToSingle(j1) + G3;
            Vector<float> z1 = z0 - Vector.ConvertToSingle(k1) + G3;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + i1, j + j1, k + k1, x1, y1, z1, t), Vector<float>.Zero);

            // Round 3
            i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, -Vector<int>.One), Vector<int>.One, Vector<int>.Zero);
            j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, -Vector<int>.One), Vector<int>.One, Vector<int>.Zero);
            k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, -Vector<int>.One), Vector<int>.One, Vector<int>.Zero);
            Vector<float> g = 2.0f * G3;
            x1 = x0 - Vector.ConvertToSingle(i1) + g;
            y1 = y0 - Vector.ConvertToSingle(j1) + g;
            z1 = z0 - Vector.ConvertToSingle(k1) + g;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + i1, j + j1, k + k1, x1, y1, z1, t), Vector<float>.Zero);

            // Round 4
            g = 3.0f * G3;
            i1 = Vector<int>.One;
            j1 = Vector<int>.One;
            k1 = Vector<int>.One;
            x1 = x0 - Vector<float>.One + g;
            y1 = y0 - Vector<float>.One + g;
            z1 = z0 - Vector<float>.One + g;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + i1, j + j1, k + k1, x1, y1, z1, t), Vector<float>.Zero);

            return n * 32.0f;
        }

        private Vector<float> Grad(Vector<int> i, Vector<int> j, Vector<int> k, Vector<float> x, Vector<float> y, Vector<float> z, Vector<float> t)
        {
            Vector<uint> hash = new Vector<uint>(Seed);
            hash ^= (Vector<uint>)(1619 * i);
            hash ^= (Vector<uint>)(31337 * j);
            hash ^= (Vector<uint>)(6971 * k);

            hash = hash * hash * hash * 60493;
            hash = (hash / new Vector<uint>(32768)) ^ hash;

            t *= t;
            return t * t * (Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & Vector<uint>.One), Vector<int>.Zero), -z, z) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(2)), Vector<int>.Zero), -y, y) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(4)), Vector<int>.Zero), -x, x));
        }

        protected Vector<float> Noise(Vector<float> xin, Vector<float> yin, Vector<float> zin, Vector<float> win)
        {
            Vector<float> s = (xin + yin + zin + win) * F4;
            Vector<int> i = FastFloor(xin + s);
            Vector<int> j = FastFloor(yin + s);
            Vector<int> k = FastFloor(zin + s);
            Vector<int> l = FastFloor(win + s);

            Vector<float> i0 = Vector.ConvertToSingle(i);
            Vector<float> j0 = Vector.ConvertToSingle(j);
            Vector<float> k0 = Vector.ConvertToSingle(k);
            Vector<float> l0 = Vector.ConvertToSingle(l);

            Vector<float> t = (i0 + j0 + k0 + l0) * G4;

            Vector<float> x0 = xin - (i0 - t);
            Vector<float> y0 = yin - (j0 - t);
            Vector<float> z0 = zin - (k0 - t);
            Vector<float> w0 = win - (l0 - t);

            Vector<int> xranks = Vector.GreaterThan(x0, y0) + Vector.GreaterThan(x0, z0) + Vector.GreaterThan(x0, w0);
            Vector<int> yranks = Vector.GreaterThan(y0, z0) + Vector.LessThanOrEqual(x0, y0) + Vector.GreaterThan(y0, w0);
            Vector<int> zranks = Vector.LessThanOrEqual(y0, z0) + Vector.LessThanOrEqual(x0, z0) + Vector.GreaterThan(z0, w0);
            Vector<int> wranks = Vector.LessThanOrEqual(y0, w0) + Vector.LessThanOrEqual(x0, w0) + Vector.LessThanOrEqual(z0, w0);

            // Round 1
            t = new Vector<float>(0.6f) - (x0 * x0) - (y0 * y0) - (z0 * z0) - (w0 * w0);
            Vector<float> n = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i, j, k, l, x0, y0, z0, w0, t), Vector<float>.Zero);

            // Round 2
            Vector<int> r = new Vector<int>(-3);
            Vector<int> i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<int> j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<int> k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<int> l1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(wranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<float> x1 = x0 - Vector.ConvertToSingle(i1) + G4;
            Vector<float> y1 = y0 - Vector.ConvertToSingle(j1) + G4;
            Vector<float> z1 = z0 - Vector.ConvertToSingle(k1) + G4;
            Vector<float> w1 = w0 - Vector.ConvertToSingle(l1) + G4;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1) - (w1 * w1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1, t), Vector<float>.Zero);

            // Round 3
            r = new Vector<int>(-2);
            i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, r), Vector<int>.One, Vector<int>.Zero);
            j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, r), Vector<int>.One, Vector<int>.Zero);
            k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, r), Vector<int>.One, Vector<int>.Zero);
            l1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(wranks, r), Vector<int>.One, Vector<int>.Zero);
            Vector<float> g2 = 2.0f * G4;
            x1 = x0 - Vector.ConvertToSingle(i1) + g2;
            y1 = y0 - Vector.ConvertToSingle(j1) + g2;
            z1 = z0 - Vector.ConvertToSingle(k1) + g2;
            w1 = w0 - Vector.ConvertToSingle(l1) + g2;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1) - (w1 * w1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1, t), Vector<float>.Zero);

            // Round 4
            r = new Vector<int>(-1);
            i1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(xranks, r), Vector<int>.One, Vector<int>.Zero);
            j1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(yranks, r), Vector<int>.One, Vector<int>.Zero);
            k1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(zranks, r), Vector<int>.One, Vector<int>.Zero);
            l1 = Vector.ConditionalSelect(Vector.LessThanOrEqual(wranks, r), Vector<int>.One, Vector<int>.Zero);
            g2 = 3.0f * G4;
            x1 = x0 - Vector.ConvertToSingle(i1) + g2;
            y1 = y0 - Vector.ConvertToSingle(j1) + g2;
            z1 = z0 - Vector.ConvertToSingle(k1) + g2;
            w1 = w0 - Vector.ConvertToSingle(l1) + g2;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1) - (w1 * w1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1, t), Vector<float>.Zero);

            // Round 5
            g2 = 4.0f * G4;
            x1 = x0 - Vector<float>.One + g2;
            y1 = y0 - Vector<float>.One + g2;
            z1 = z0 - Vector<float>.One + g2;
            w1 = w0 - Vector<float>.One + g2;

            t = new Vector<float>(0.6f) - (x1 * x1) - (y1 * y1) - (z1 * z1) - (w1 * w1);
            n += Vector.ConditionalSelect(Vector.GreaterThanOrEqual(t, Vector<float>.Zero), Grad(i + Vector<int>.One, j + Vector<int>.One, k + Vector<int>.One, l + Vector<int>.One, x1, y1, z1, w1, t), Vector<float>.Zero);

            return n * 32.0f;
        }

        private Vector<float> Grad(Vector<int> i, Vector<int> j, Vector<int> k, Vector<int> l, Vector<float> x, Vector<float> y, Vector<float> z, Vector<float> w, Vector<float> t)
        {
            Vector<uint> hash = new Vector<uint>(Seed);
            hash ^= (Vector<uint>)(1619 * i);
            hash ^= (Vector<uint>)(31337 * j);
            hash ^= (Vector<uint>)(6971 * k);
            hash ^= (Vector<uint>)(1013 * l);

            hash = hash * hash * hash * 60493;
            hash = (hash / new Vector<uint>(32768)) ^ hash;

            t *= t;
            return t * t * (Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & Vector<uint>.One), Vector<int>.Zero), -w, w) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(2)), Vector<int>.Zero), -z, z) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(4)), Vector<int>.Zero), -y, y) + Vector.ConditionalSelect(Vector.Equals((Vector<int>)(hash & new Vector<uint>(8)), Vector<int>.Zero), -x, x));
        }

        private static Vector<int> FastFloor(Vector<float> vals)
        {
#if NET5_0_OR_GREATER
            return Vector.ConvertToInt32(Vector.Floor(vals));
#else
            Vector<int> xi = Vector.ConvertToInt32(vals);

            return Vector.ConditionalSelect(Vector.LessThan(vals, Vector.ConvertToSingle(xi)), xi - Vector<int>.One, xi);
#endif
        }
    }
}
