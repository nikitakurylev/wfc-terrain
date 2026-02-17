using System.Collections.Generic;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration.Interpolators
{
    public class BarycentricInterpolator : ITerrainInterpolator
    {
        DelaunayTriangulation _triangulation;

        public BarycentricInterpolator(Biome[,] biomes, Vector2Int[,] centers)
        {
            var vertices = new List<DelaunayTriangulation.Vertex>();
            for (int i = 0; i < biomes.GetLength(0); i++)
            for (int j = 0; j < biomes.GetLength(1); j++)
            {
                vertices.Add(new DelaunayTriangulation.Vertex
                {
                    position = centers[i, j],
                    biome = biomes[i, j]
                });
            }
            _triangulation = new DelaunayTriangulation(vertices);
        }

        public Dictionary<Biome, float> ComputeWeights(Vector2Int p)
        {
            var triangle = _triangulation.FindContainingTriangle(p);
            if (!triangle.HasValue)
                return new Dictionary<Biome, float>();
            var t = triangle.Value;

            if (t.a.biome == t.b.biome && t.b.biome == t.c.biome)
                return new Dictionary<Biome, float> { { t.a.biome, 1f } };
            
            Vector2 a = t.a.position;
            Vector2 b = t.b.position;
            Vector2 c = t.c.position;

            float area = Cross(b - a, c - a);

            float w1 = Cross(b - p, c - p) / area;
            float w2 = Cross(c - p, a - p) / area;
            float w3 = 1f - w1 - w2;
            var dict = new Dictionary<Biome, float>();

            // Add t.a
            if (!dict.TryAdd(t.a.biome, w1))
                dict[t.a.biome] += w1;

            // Add t.b
            if (!dict.TryAdd(t.b.biome, w2))
                dict[t.b.biome] += w2;

            // Add t.c
            if (!dict.TryAdd(t.c.biome, w3))
                dict[t.c.biome] += w3;

            return dict;
        }

        static float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }
    }
}