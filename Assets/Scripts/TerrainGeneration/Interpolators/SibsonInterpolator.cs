using System.Collections.Generic;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public class SibsonInterpolator : ITerrainInterpolator
    {
        private readonly int _biomeSize;
        private readonly List<(Vector2 pos, Biome biome)>[,] _neighbours;

        public SibsonInterpolator(Biome[,] biomes, Vector2Int[,] centers, int biomeSize)
        {
            _biomeSize = biomeSize;
            _neighbours = new List<(Vector2 pos, Biome biome)>[biomes.GetLength(0), biomes.GetLength(1)];
            for (int i = 0; i < biomes.GetLength(0); i++)
            {
                for (int j = 0; j < biomes.GetLength(1); j++)
                {
                    _neighbours[i, j] = CollectNeighbours(
                        centers,
                        biomes,
                        i,
                        j,
                        biomes.GetLength(0)
                    );
                }
            }
        }

        public (Biome, float)[] ComputeWeights(Vector2Int p)
        {
            var neighbours = _neighbours[(p.x - 1) / _biomeSize, (p.y - 1) / _biomeSize];

            // Initial infinite Voronoi cell (big square)
            List<Vector2> cell = CreateBoundingBox(p, 10000f);

            // Clip against each neighbour bisector
            foreach (var n in neighbours)
            {
                cell = ClipCell(cell, p, n.pos);

                if (cell.Count == 0)
                    break;
            }

            var areas = new List<(Biome, float)>();
            float total = 0f;

            foreach (var n in neighbours)
            {
                var subCell = new List<Vector2>(cell);

                foreach (var other in neighbours)
                {
                    if (other.pos == n.pos) continue;

                    subCell = ClipCell(
                        subCell,
                        n.pos,
                        other.pos
                    );

                    if (subCell.Count == 0)
                        break;
                }

                float a = PolygonArea(subCell);

                if (a > 0)
                {
                    areas.Add((n.biome, a));
                    total += a;
                }
            }

            var result = new (Biome, float)[areas.Count];
            
            // Normalize
            for (var i = 0; i < areas.Count; i++)
                result[i] = (areas[i].Item1, areas[i].Item2 / total);

            return result;
        }

        static List<Vector2> CreateBoundingBox(Vector2 c, float r)
        {
            return new List<Vector2>
            {
                c + new Vector2(-r, -r),
                c + new Vector2(-r, r),
                c + new Vector2(r, r),
                c + new Vector2(r, -r)
            };
        }

        // Half-plane clipping (Sutherland–Hodgman)
        static List<Vector2> ClipCell(
            List<Vector2> poly,
            Vector2 p,
            Vector2 site)
        {
            List<Vector2> output = new();

            Vector2 mid = (p + site) * 0.5f;
            Vector2 n = (site - p).normalized;

            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 a = poly[i];
                Vector2 b = poly[(i + 1) % poly.Count];

                bool aIn = Vector2.Dot(a - mid, n) <= 0;
                bool bIn = Vector2.Dot(b - mid, n) <= 0;

                if (aIn && bIn)
                {
                    output.Add(b);
                }
                else if (aIn && !bIn)
                {
                    output.Add(Intersect(a, b, mid, n));
                }
                else if (!aIn && bIn)
                {
                    output.Add(Intersect(a, b, mid, n));
                    output.Add(b);
                }
            }

            return output;
        }

        static Vector2 Intersect(
            Vector2 a,
            Vector2 b,
            Vector2 mid,
            Vector2 n)
        {
            Vector2 ab = b - a;
            float t =
                Vector2.Dot(mid - a, n) /
                Vector2.Dot(ab, n);

            return a + t * ab;
        }

        static float PolygonArea(List<Vector2> poly)
        {
            float area = 0f;

            for (int i = 0; i < poly.Count; i++)
            {
                var a = poly[i];
                var b = poly[(i + 1) % poly.Count];

                area += (a.x * b.y - b.x * a.y);
            }

            return Mathf.Abs(area) * 0.5f;
        }

        static float ComputeOverlap(
            List<Vector2> cell,
            Vector2 p,
            List<Vector2> neighbours)
        {
            // Sibson area ≈ full cell area / neighbour count
            return PolygonArea(cell) / neighbours.Count;
        }

        static List<(Vector2 pos, Biome biome)> CollectNeighbours(
            Vector2Int[,] centers,
            Biome[,] biomes,
            int cx,
            int cy,
            int size,
            int maxRadius = 2 // usually enough
        )
        {
            List<(Vector2, Biome)> result = new();

            float maxInfluence = float.PositiveInfinity;

            for (int r = 0; r <= maxRadius; r++)
            {
                bool added = false;

                for (int i = cx - r; i <= cx + r; i++)
                for (int j = cy - r; j <= cy + r; j++)
                {
                    if (i < 0 || j < 0 || i >= size || j >= size)
                        continue;

                    // Only border of square (avoid duplicates)
                    if (Mathf.Abs(i - cx) != r &&
                        Mathf.Abs(j - cy) != r)
                        continue;

                    Vector2 c = centers[i, j];

                    result.Add((c, biomes[i, j]));
                    added = true;
                }

                // If no new useful points, stop
                if (!added && r > 2)
                    break;
            }

            return result;
        }
    }
}