using TerrainGeneration.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

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

        public void GenerateTerrain()
        {
            float[,] _heights;
            var startTime = Time.realtimeSinceStartup;

            var terrainData = _terrain.terrainData;
            _heights = _terrain.terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                terrainData.heightmapResolution);

            int width = _heights.GetLength(0);
            int length = _heights.GetLength(1);
            TileScale = terrainData.size.x / width;
            var biomeMapSize = width / _settings.BiomeSize + 1;

            var biomes = _waveFunctionCollapse.GenerateBiomes(biomeMapSize);

            var alphaMaps = new float[terrainData.alphamapResolution, terrainData.alphamapResolution,
                terrainData.alphamapLayers];
            for (var i = 0; i < width; i++)
            {
                var horizontalT = (i - 1) % _settings.BiomeSize / (float)_settings.BiomeSize;
                var biomeX = (i - 1) / _settings.BiomeSize;
                for (var j = 0; j < length; j++)
                {
                    var verticalT = (j - 1) % _settings.BiomeSize / (float)_settings.BiomeSize;
                    var biomeY = (j - 1) / _settings.BiomeSize;
                    _heights[i, j] = 0f;

                    var topLeft = biomes[biomeX, biomeY];
                    var topRight = biomes[biomeX + 1, biomeY];
                    var bottomLeft = biomes[biomeX, biomeY + 1];
                    var bottomRight = biomes[biomeX + 1, biomeY + 1];

                    var lerp = BiLerp(
                        horizontalT, verticalT
                    );

                    var cachedPerlin = new CachedPerlinNoise(i, j);
                    _heights[i, j] =
                        lerp.x * topLeft.OctaveAmplitudes[0] +
                        lerp.y * topRight.OctaveAmplitudes[0] +
                        lerp.z * bottomLeft.OctaveAmplitudes[0] +
                        lerp.w * bottomRight.OctaveAmplitudes[0];
                    for (var octaveIndex = 1; octaveIndex < _settings.OctaveScales.Count; octaveIndex++)
                    {
                        var octaveScale = _settings.OctaveScales[octaveIndex];
                        if (octaveScale == 0)
                            continue;
                        var biomeFactor =
                            lerp.x * topLeft.OctaveAmplitudes[octaveIndex] +
                            lerp.y * topRight.OctaveAmplitudes[octaveIndex] +
                            lerp.z * bottomLeft.OctaveAmplitudes[octaveIndex] +
                            lerp.w * bottomRight.OctaveAmplitudes[octaveIndex];
                        _heights[i, j] += cachedPerlin.GetValue(octaveScale) * biomeFactor;
                    }

                    if (i < terrainData.alphamapResolution && j < terrainData.alphamapResolution)
                    {
                        var height = _heights[i, j];
                        AddTerrainLayerAlpha(alphaMaps, i, j, lerp.x, topLeft, height);
                        AddTerrainLayerAlpha(alphaMaps, i, j, lerp.y, topRight, height);
                        AddTerrainLayerAlpha(alphaMaps, i, j, lerp.z, bottomLeft, height);
                        AddTerrainLayerAlpha(alphaMaps, i, j, lerp.w, bottomRight, height);
                    }
                }
            }

            Debug.Log($"Terrain Generation took {Time.realtimeSinceStartup - startTime} seconds");
            _terrain.terrainData.SetHeights(0, 0, _heights);

            _terrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
        }

        private void AddTerrainLayerAlpha(float[,,] alphaMaps, int i, int j, float biomeStrength, Biome biome, float height)
        {
            var colorStrength = biome.GetColorInterpolation(height);

            alphaMaps[i, j, biome.Color1] += biomeStrength * (1 - colorStrength);
            alphaMaps[i, j, biome.Color2] += biomeStrength * colorStrength;
        }

        private Vector4 BiLerp(float horizontalT, float verticalT)
        {
            return Vector4.Lerp(
                Vector4.Lerp(new Vector4(1, 0, 0, 0),
                    new Vector4(0, 1, 0, 0),
                    _settings.InterpolationCurve.Evaluate(horizontalT)),
                Vector4.Lerp(new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1),
                    _settings.InterpolationCurve.Evaluate(horizontalT)),
                _settings.InterpolationCurve.Evaluate(verticalT));
        }
    }
}