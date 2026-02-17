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

        public float TotalTime { get; private set; }
        public float WfcTime { get; private set; }
        public float PerlinTime { get; private set; }
        public float PaintTime { get; private set; }

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
            var heights = _terrain.terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                terrainData.heightmapResolution);

            int width = settings.Size;
            int length = settings.Size;
            TileScale = terrainData.size.x / width;
            var biomeMapSize = Mathf.CeilToInt(1f * width / settings.BiomeSize) + 1;

            WfcTime = Time.realtimeSinceStartup;
            var biomes = _waveFunctionCollapse.GenerateBiomes(biomeMapSize);
            var biomeCenter = new Vector2Int[biomeMapSize, biomeMapSize];

            for (int i = 0; i < biomeMapSize; i++)
            for (int j = 0; j < biomeMapSize; j++)
            {
                biomeCenter[i, j] = new Vector2Int(
                    i * settings.BiomeSize + Random.Range(-settings.BiomeSize / 2, settings.BiomeSize / 2),
                    j * settings.BiomeSize + Random.Range(-settings.BiomeSize / 2, settings.BiomeSize / 2));
            }
            
            WfcTime = Time.realtimeSinceStartup - WfcTime;
            var alphaMaps = new float[terrainData.alphamapResolution, terrainData.alphamapResolution,
                terrainData.alphamapLayers];

            ITerrainInterpolator interpolator = settings.InterpolatorType switch {
                InterpolatorType.Linear => new LinearInterpolator(biomes, settings.BiomeSize, settings.InterpolationCurve),
                InterpolatorType.Barycentric => new BarycentricInterpolator(biomes, biomeCenter),
                InterpolatorType.Sibson => new SibsonInterpolator(biomes, biomeCenter, settings.BiomeSize),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            for (var i = 0; i < width; i++)
            {
                
                for (var j = 0; j < length; j++)
                {

                    heights[i, j] = 0f;

                    var weights = interpolator.ComputeWeights(new Vector2Int(i, j));

                    var cachedPerlin = new CachedPerlinNoise(i, j);
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
                    if (i < terrainData.alphamapResolution && j < terrainData.alphamapResolution)
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

            _terrain.terrainData.SetHeights(0, 0, heights);
            _terrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
            TotalTime = Time.realtimeSinceStartup - startTime;
            PerlinTime = TotalTime - PaintTime - WfcTime;
            Debug.Log(TotalTime);
        }

        private void AddTerrainLayerAlpha(float[,,] alphaMaps, int i, int j, float biomeStrength, Biome biome, float height)
        {
            var colorStrength = biome.GetColorInterpolation(height);

            alphaMaps[i, j, biome.Color1] += biomeStrength * (1 - colorStrength);
            alphaMaps[i, j, biome.Color2] += biomeStrength * colorStrength;
        }
    }
}