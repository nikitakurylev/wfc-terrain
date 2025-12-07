using System.Collections.Generic;
using System.Linq;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Vector2 = System.Numerics.Vector2;

namespace TerrainGeneration
{
    public class TerrainManager : MonoBehaviour
    {
        [SerializeField] private Terrain terrain;
        [SerializeField] private float maxHeightDifference;
        [SerializeField] private WaveFunctionCollapseGenerator waveFunctionCollapseGenerator;

        [FormerlySerializedAs("terrainGenerationSettings")] [SerializeField]
        private TerrainGenerationSettings settings;

        public float TileScale { get; private set; }

        private float[,] _heights;
        private float _yOffset;
        int _width;
        int _length;

        public void GenerateTerrain()
        {
            _yOffset = terrain.transform.position.y;

            var terrainData = terrain.terrainData;
            var layers = terrainData.terrainLayers.ToList();
            _heights = terrain.terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                terrainData.heightmapResolution);

            _width = _heights.GetLength(0);
            _length = _heights.GetLength(1);
            TileScale = terrainData.size.x / _width;
            var biomeMapSize = _width / settings.BiomeSize + 1;

            var biomes = waveFunctionCollapseGenerator.Generate(biomeMapSize);
            var offsets = new Vector2[biomeMapSize, biomeMapSize];
            for (int i = 0; i < biomeMapSize; i++)
            {
                for (int j = 0; j < biomeMapSize; j++)
                {
                    offsets[i, j] = new Vector2(Random.Range(-settings.MaxRandomOffset, settings.MaxRandomOffset), Random.Range(-settings.MaxRandomOffset, settings.MaxRandomOffset));
                }
            }

            var alphaMaps = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, terrainData.alphamapLayers];
            for (var i = 0; i < _width; i++)
            {
                var horizontalT = (i - 1) % settings.BiomeSize / (float)settings.BiomeSize;
                var biomeX = (i - 1) / settings.BiomeSize;
                for (var j = 0; j < _length; j++)
                {
                    var verticalT = (j - 1) % settings.BiomeSize / (float)settings.BiomeSize;
                    var biomeY = (j - 1) / settings.BiomeSize;
                    var offset = BiLerp(
                        offsets[biomeX, biomeY], 
                        offsets[biomeX + 1, biomeY],
                        offsets[biomeX, biomeY + 1], 
                        offsets[biomeX + 1, biomeY + 1], 
                        horizontalT, verticalT);
                    _heights[i, j] = 0f;

                    var topLeft = biomes[biomeX, biomeY];
                    var topRight = biomes[biomeX + 1, biomeY];
                    var bottomLeft = biomes[biomeX, biomeY + 1];
                    var bottomRight = biomes[biomeX + 1, biomeY + 1];

                    var alphas = BiLerp(
                        new Vector4(1, 0, 0, 0),
                        new Vector4(0, 1, 0, 0),
                        new Vector4(0, 0, 1, 0),
                        new Vector4(0, 0, 0, 1),
                        horizontalT + offset.X, verticalT + offset.Y
                    );

                    for (var octaveIndex = 0; octaveIndex < settings.OctaveScales.Count; octaveIndex++)
                    {
                        var octaveScale = settings.OctaveScales[octaveIndex];
                        var biomeFactor = BiLerp(
                            topLeft.OctaveAmplitudes[octaveIndex],
                            topRight.OctaveAmplitudes[octaveIndex],
                            bottomLeft.OctaveAmplitudes[octaveIndex],
                            bottomRight.OctaveAmplitudes[octaveIndex],
                            horizontalT + offset.X, verticalT + offset.Y);
                        _heights[i, j] += octaveIndex == 0 ? biomeFactor : Mathf.PerlinNoise(i * octaveScale, j * octaveScale) * biomeFactor;
                    }

                    if (i < terrainData.alphamapResolution && j < terrainData.alphamapResolution)
                    {
                        AddTerrainLayerAlpha(alphaMaps, i, j, alphas.x, layers, topLeft, _heights[i, j]);
                        AddTerrainLayerAlpha(alphaMaps, i, j, alphas.y, layers, topRight, _heights[i, j]);
                        AddTerrainLayerAlpha(alphaMaps, i, j, alphas.z, layers, bottomLeft, _heights[i, j]);
                        AddTerrainLayerAlpha(alphaMaps, i, j, alphas.w, layers, bottomRight, _heights[i, j]);
                    }
                }
            }

            terrain.terrainData.SetHeights(0, 0, _heights);

            terrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
        }

        private void AddTerrainLayerAlpha(float[,,] alphaMaps, int i, int j, float t, IList<TerrainLayer> layers, Biome biome, float height)
        {
            foreach (var layer in biome.GetLayers(height))
                alphaMaps[i, j, layers.IndexOf(layer.Layer)] += t * layer.Value;
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            var terrainData = terrain.terrainData;
            return new Vector3(
                position.y * terrainData.size.x / terrainData.heightmapResolution,
                _heights[position.x, position.y] * terrainData.size.y + _yOffset,
                position.x * terrainData.size.z / terrainData.heightmapResolution);
        }

        public bool IsInBounds(Vector2Int position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < _width && position.y < _length;
        }

        private float BiLerp(float topLeft, float topRight, float bottomLeft, float bottomRight, float horizontalT,
            float verticalT)
        {
            return Mathf.Lerp(
                Mathf.Lerp(topLeft,
                    topRight,
                    settings.InterpolationCurve.Evaluate(horizontalT)),
                Mathf.Lerp(bottomLeft,
                    bottomRight,
                    settings.InterpolationCurve.Evaluate(horizontalT)),
                settings.InterpolationCurve.Evaluate(verticalT));
        }

        private Vector2 BiLerp(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, float horizontalT,
            float verticalT)
        {
            return Vector2.Lerp(
                Vector2.Lerp(topLeft,
                    topRight,
                    settings.InterpolationCurve.Evaluate(horizontalT)),
                Vector2.Lerp(bottomLeft,
                    bottomRight,
                    settings.InterpolationCurve.Evaluate(horizontalT)),
                settings.InterpolationCurve.Evaluate(verticalT));
        }

        private Vector4 BiLerp(Vector4 topLeft, Vector4 topRight, Vector4 bottomLeft, Vector4 bottomRight, float horizontalT,
            float verticalT)
        {
            return Vector4.Lerp(
                Vector4.Lerp(topLeft,
                    topRight,
                    settings.InterpolationCurve.Evaluate(horizontalT)),
                Vector4.Lerp(bottomLeft,
                    bottomRight,
                    settings.InterpolationCurve.Evaluate(horizontalT)),
                settings.InterpolationCurve.Evaluate(verticalT));
        }
    }
}