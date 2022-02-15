using CP.Procedural.Maths;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CP.Procedural.PoissonDisc
{
    public partial class PoissonDiscSampling
    {
        private static readonly float pi = 2 * (float)Math.PI;

        public static List<PoissonDisc> Sample2D(int seed, float minRadius, Vector3 regionSize, Vector3 centerPos, int k = 4, bool createNeighbours = false, Func<float, float, float> calcRadius = null)
        {
            List<PoissonDisc> points = new List<PoissonDisc>();
            float maxRadius = 0f;
            int searchZone = 2;

            Random hash = new Random(seed);
            float cellSize = minRadius * 2 / 1.414213f; // Minimum distance between cells divided by sqrt(2)

            int[,] grid = new int[(int)Math.Ceiling(regionSize.X / cellSize), (int)Math.Ceiling(regionSize.Y / cellSize)];
            List<PoissonDisc> spawnPoints = new List<PoissonDisc>
            {
                new PoissonDisc(centerPos, minRadius)
            };

            points.Add(spawnPoints[0]);
            grid[(int)(spawnPoints[0].position.X / cellSize), (int)(spawnPoints[0].position.Y / cellSize)] = points.Count;

            while (spawnPoints.Count > 0)
            {
                int spawnIndex = hash.Next(0, spawnPoints.Count);
                PoissonDisc spawnCentre = spawnPoints[spawnIndex];

                bool candidateAccepted = false;
                float randRot = (float)hash.NextDouble();
                float nextRadius = calcRadius == null ? minRadius : calcRadius(spawnCentre.position.X, spawnCentre.position.Y);
                float distance = spawnCentre.radius + nextRadius;
                float r = distance + float.Epsilon;

                Vector<float> K = new Vector<float>(k);
                Vector<float> J = new Vector<float>(new float[8] { 0,1,2,3,4,5,6,7 });

                Vector<float> theta = Vector.Multiply(new Vector<float>(randRot + 1.0f), Vector.Divide(J, K));

                Vector<float> x = Vector.Add(new Vector<float>(spawnCentre.position.X), Vector.Multiply(r, VectorTrig.Cos(theta)));
                Vector<float> y = Vector.Add(new Vector<float>(spawnCentre.position.Y), Vector.Multiply(r, VectorTrig.Sin(theta)));
                


                for (int j = 0; j < k; j++)
                {
                    float theta = pi * (randRot + 1.0f * j / k);

                    float x = spawnCentre.X + r * (float)Math.Cos(theta);
                    float y = spawnCentre.Y + r * (float)Math.Sin(theta);

                    Vector3 candidate = new Vector3(x, y, 0);
                    if (IsValid2D(candidate, nextRadius, searchZone, regionSize, cellSize, grid, points))
                    {
                        if (distance > maxRadius)
                        {
                            maxRadius = distance;
                            searchZone = (int)Math.Ceiling(distance / cellSize);
                        }
                        PoissonDisc disc = new PoissonDisc(candidate, nextRadius);
                        points.Add(disc);
                        spawnPoints.Add(disc);
                        grid[(int)(candidate.X / cellSize), (int)(candidate.Y / cellSize)] = points.Count;
                        candidateAccepted = true;
                        break;
                    }
                }
                if (!candidateAccepted)
                    spawnPoints.RemoveAt(spawnIndex);
            }

            if (createNeighbours)
                return CreateNeighbourList2D(points, grid, searchZone);

            return points;
        }

        private static bool IsValid2D(ref Vector<float> x, ref Vector<float> y, double radius, int searchZone, Vector3 sampleRegionSize, double cellSize, int[,] grid, List<PoissonDisc> points)
        {
            x = Vector.ConditionalSelect(Vector.LessThan(x, Vector<float>.Zero), Vector<float>.Zero, x);
            x = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(x, new Vector<float>(sampleRegionSize.X)), Vector<float>.Zero, x);

            y = Vector.ConditionalSelect(Vector.LessThan(y, Vector<float>.Zero), Vector<float>.Zero, y);
            y = Vector.ConditionalSelect(Vector.GreaterThanOrEqual(y, new Vector<float>(sampleRegionSize.Y)), Vector<float>.Zero, y);



            int cellX = (int)(candidate.X / cellSize);
            int cellY = (int)(candidate.Y / cellSize);
            int searchStartX = Math.Max(0, cellX - searchZone);
            int searchEndX = Math.Min(cellX + searchZone, grid.GetLength(0) - 1);
            int searchStartY = Math.Max(0, cellY - searchZone);
            int searchEndY = Math.Min(cellY + searchZone, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        PoissonDisc disc = points[pointIndex];
                        double sqrDst = (candidate - disc.position).SqrMagnitude;
                        double r = disc.radius + radius;
                        if (sqrDst < r * r)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static List<PoissonDisc> CreateNeighbourList2D(List<PoissonDisc> points, int[,] grid, int searchZone)
        {
            List<PoissonDisc> newPoints = new List<PoissonDisc>();

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        PoissonDisc disc = points[pointIndex];
                        newPoints.Add(disc);

                        int searchStartX = Math.Max(0, x - searchZone);
                        int searchEndX = Math.Min(x + searchZone, grid.GetLength(0) - 1);
                        int searchStartY = Math.Max(0, y - searchZone);
                        int searchEndY = Math.Min(y + searchZone, grid.GetLength(1) - 1);

                        for (int x1 = searchStartX; x1 <= searchEndX; x1++)
                        {
                            for (int y1 = searchStartY; y1 <= searchEndY; y1++)
                            {
                                int pointIndex2 = grid[x1, y1] - 1;
                                if (pointIndex2 != -1 && pointIndex2 != pointIndex)
                                {
                                    PoissonDisc disc2 = points[pointIndex2];

                                    double sqrDst = (disc2.position - disc.position).SqrMagnitude;
                                    if (sqrDst < disc.radiusSquared * 2)
                                    {
                                        disc.AddNeighbour(disc2);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return newPoints;
        }
    }
}
