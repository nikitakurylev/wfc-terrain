using System;
using System.Collections.Generic;
using System.Linq;
using TerrainGeneration.Interpolators;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TerrainGeneration
{
    public class TerrainManager : MonoBehaviour
    {
        [FormerlySerializedAs("terrain")] [SerializeField]
        private Terrain _terrain;

        [FormerlySerializedAs("waveFunctionCollapse")]
        [FormerlySerializedAs("waveFunctionCollapseGenerator")]
        [SerializeField]
        private WaveFunctionCollapseGenerator _waveFunctionCollapse;

        [FormerlySerializedAs("settings")] [FormerlySerializedAs("terrainGenerationSettings")] [SerializeField]
        private TerrainGenerationSettings _settings;

        public float TileScale { get; private set; }
        public float ScaledBiomeSize { get; private set; }

        public float TotalTime { get; private set; }
        public float WfcTime { get; private set; }
        public float PerlinTime { get; private set; }
        public float PaintTime { get; private set; }

        private Biome[,] biomes;
        private Vector2Int[,] biomeCenter;
        
        public void GenerateTerrain()
        {
            GenerateTerrain(_settings);
        }

        public void GenerateTerrain(ITerrainGenerationSettings settings)
        {
            TotalTime = 0;
            WfcTime = 0;
            PerlinTime = 0;
            PaintTime = 0;

            var startTime = Time.realtimeSinceStartup;

            var terrainData = _terrain.terrainData;

            int width = settings.Size;
            int length = settings.Size;
            TileScale = terrainData.size.x / width;
            ScaledBiomeSize = TileScale * _settings.BiomeSize;
            var biomeMapSize = Mathf.CeilToInt(1f * width / settings.BiomeSize) + 1;

            WfcTime = Time.realtimeSinceStartup;
            biomes = _waveFunctionCollapse.GenerateBiomes(biomeMapSize);
            biomeCenter = new Vector2Int[biomeMapSize, biomeMapSize];

            for (int i = 0; i < biomeMapSize; i++)
            for (int j = 0; j < biomeMapSize; j++)
            {
                biomeCenter[i, j] = new Vector2Int(
                    i * settings.BiomeSize + Random.Range(-settings.BiomeSize / 2, settings.BiomeSize / 2),
                    j * settings.BiomeSize + Random.Range(-settings.BiomeSize / 2, settings.BiomeSize / 2));
            }

            WfcTime = Time.realtimeSinceStartup - WfcTime;

            GenerateHeightMap(settings, 0, 0, width, length);

            TotalTime = Time.realtimeSinceStartup - startTime;
            PerlinTime = TotalTime - PaintTime - WfcTime;
            Debug.Log(TotalTime);
        }

        public void RegenerateTerrain(Vector2Int from, Vector2Int to)
        {
            
            
            for (int i = from.x; i < to.x; i++)
            for (int j = from.y; j < to.y; j++)
            {
                biomes[j, i] = _settings.Biomes[0];
            }
            
            var startX = (from.x - 1) * _settings.BiomeSize;
            var endX = to.x * _settings.BiomeSize;
            var startY = (from.y - 1) * _settings.BiomeSize;
            var endY = to.y * _settings.BiomeSize;
            var sizeX = endX - startX;
            var sizeY = endY - startY;

            GenerateHeightMap(_settings, startX, startY, sizeY, sizeX);
        }

        private void GenerateHeightMap(ITerrainGenerationSettings settings,
            int startX, int startY, int width, int length)
        {
            ITerrainInterpolator interpolator = settings.InterpolatorType switch
            {
                InterpolatorType.Linear => new LinearInterpolator(biomes, settings.BiomeSize,
                    settings.InterpolationCurve),
                InterpolatorType.Barycentric => new BarycentricInterpolator(biomes, biomeCenter),
                InterpolatorType.Sibson => new SibsonInterpolator(biomes, biomeCenter, settings.BiomeSize),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            var terrainData = _terrain.terrainData;
            var alphaMaps = new float[Mathf.Min(terrainData.alphamapResolution, width),
                Mathf.Min(terrainData.alphamapResolution, length),
                terrainData.alphamapLayers];
            var heights = new float[width, length];

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    heights[i, j] = 0f;

                    var x = startY + i;
                    var y = startX + j;

                    var weights = interpolator.ComputeWeights(new Vector2Int(x, y));

                    var cachedPerlin = new CachedPerlinNoise(x, y);
                    heights[i, j] = weights.Sum(w => w.Key.OctaveAmplitudes[0] * w.Value);
                    for (var octaveIndex = 1; octaveIndex < settings.OctaveScales.Count; octaveIndex++)
                    {
                        var octaveScale = settings.OctaveScales[octaveIndex];
                        if (octaveScale == 0)
                            continue;
                        var biomeFactor = weights.Sum(w => w.Key.OctaveAmplitudes[octaveIndex] * w.Value);
                        heights[i, j] += cachedPerlin.GetValue(octaveScale) * biomeFactor;
                    }

                    var localPaintTime = Time.realtimeSinceStartup;
                    if (x < terrainData.alphamapResolution && y < terrainData.alphamapResolution)
                    {
                        var height = heights[i, j];
                        foreach (var weight in weights)
                        {
                            AddTerrainLayerAlpha(alphaMaps, i, j, weight.Value, weight.Key, height);
                        }
                    }

                    PaintTime += Time.realtimeSinceStartup - localPaintTime;
                }
            }

            _terrain.terrainData.SetHeights(startX, startY, heights);
            _terrain.terrainData.SetAlphamaps(startX, startY, alphaMaps);
        }

        private void AddTerrainLayerAlpha(float[,,] alphaMaps, int i, int j, float biomeStrength, Biome biome,
            float height)
        {
            var colorStrength = biome.GetColorInterpolation(height);

            alphaMaps[i, j, biome.Color1] += biomeStrength * (1 - colorStrength);
            alphaMaps[i, j, biome.Color2] += biomeStrength * colorStrength;
        }
    }
}