using System;
using System.Collections.Generic;
using System.Text;

namespace CP.Procedural.Noise
{
    public class NoiseGen2
    {
        private uint seed;
        private float persistence;
        private float scale;
        public virtual uint Seed { get => seed; protected set => seed = value; }
        public virtual float Persistence { get => persistence; set => persistence = value; }
        public virtual float Scale { get => scale; set => scale = value; }
        public NoiseGen2(uint seed)
        {
            Seed = seed;
        }


    }
}
