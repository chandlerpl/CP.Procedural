using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CP.Procedural.PoissonDisc
{
    public class PoissonDisc
    {
        public Vector3 position;
        public float radius;
        public float radiusSquared;
        List<PoissonDisc> neighbours;

        public PoissonDisc(Vector3 position, float radius)
        {
            this.position = position;

            this.radius = radius;
            this.radiusSquared = radius * radius;
        }

        public void AddNeighbour(PoissonDisc neighbour)
        {
            if (neighbours == null)
                neighbours = new List<PoissonDisc>();

            neighbours.Add(neighbour);
        }

        public List<PoissonDisc> GetNeighbours()
        {
            return neighbours;
        }

        public int GetNeighbourCount()
        {
            return neighbours.Count;
        }
    }
}
