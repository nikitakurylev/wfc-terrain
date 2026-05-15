using System.Collections.Generic;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public class SibsonInterpolator : ITerrainInterpolator
    {
        private readonly int _biomeSize;
        private readonly Vector2Int[,] _centers;
        private readonly Biome[,] _biomes;
        private readonly List<(Vector2 pos, Biome biome)>[,] _neighbours;

        public SibsonInterpolator(Biome[,] biomes, Vector2Int[,] centers, int biomeSize)
        {
            _biomeSize = biomeSize;
            _neighbours = new List<(Vector2 pos, Biome biome)>[biomes.GetLength(0), biomes.GetLength(1)];
            _centers = centers;
            _biomes = biomes;
        }

        public (Biome, float)[] ComputeWeights(Vector2Int p)
        {
            var neighbours = CollectNeighbours(p);

            var cell = InfiniteCell(p);

            foreach (var n in neighbours)
            {
                cell = ClipCell(cell, p, n.pos);

                if (cell.Count == 0)
                    break;
            }

            var areas = new List<(Biome, float)>();
            var total = 0f;

            foreach (var n in neighbours)
            {
                var subCell = new List<Vector2>(cell);

                foreach (var other in neighbours)
                {
                    if (other.pos == n.pos) continue;

                    subCell = ClipCell(subCell, n.pos, other.pos);

                    if (subCell.Count == 0)
                        break;
                }

                var a = PolygonArea(subCell);

                if (a <= 0) continue;
                
                areas.Add((n.biome, a));
                total += a;
            }

            var result = new (Biome, float)[areas.Count];

            for (var i = 0; i < areas.Count; i++)
                result[i] = (areas[i].Item1, areas[i].Item2 / total);

            return result;
        }

        private static List<Vector2> ClipCell(
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
                else if (aIn)
                {
                    output.Add(Intersect(a, b, mid, n));
                }
                else if (bIn)
                {
                    output.Add(Intersect(a, b, mid, n));
                    output.Add(b);
                }
            }

            return output;
        }
        
        private List<Vector2> InfiniteCell(Vector2Int p) => new List<Vector2>
        {
            p + new Vector2(-10000f, -10000f),
            p + new Vector2(-10000f, 10000f),
            p + new Vector2(10000f, 10000f),
            p + new Vector2(10000f, -10000f)
        };

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

        private List<(Vector2 pos, Biome biome)> CollectNeighbours(Vector2Int point)
        {
            var biomeX = (point.x - 1) / _biomeSize;
            var biomeY = (point.y - 1) / _biomeSize;
            var size = _biomes.GetLength(0);
            
            List<(Vector2, Biome)> result = new();

            for (var i = biomeX - 2; i <= biomeX + 2; i++)
            for (var j = biomeY - 2; j <= biomeY + 2; j++)
            {
                if (i < 0 || j < 0 || i >= size || j >= size)
                    continue;

                var c = _centers[i, j];
                
                if ((c - point).magnitude * _biomeSize / 2 > _biomeSize * _biomeSize)
                    continue;
                
                result.Add((c, _biomes[i, j]));
            }

            return result;
        }
    }
}