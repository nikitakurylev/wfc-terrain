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

        private bool[,] _canBuild;
        private int[,] _horizontalRightFreeSpace;
        private int[,] _verticalFreeSpace;
        private float[,] _heights;
        private float _yOffset;
        int _width;
        int _length;

        public void GenerateTerrain()
        {
            _yOffset = terrain.transform.position.y;

            var terrainData = terrain.terrainData;
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
                    for (var octaveIndex = 0; octaveIndex < settings.OctaveScales.Count; octaveIndex++)
                    {
                        var octaveScale = settings.OctaveScales[octaveIndex];
                        var biomeFactor = BiLerp(
                            biomes[biomeX, biomeY].OctaveAmplitudes[octaveIndex],
                            biomes[biomeX + 1, biomeY].OctaveAmplitudes[octaveIndex],
                            biomes[biomeX, biomeY + 1].OctaveAmplitudes[octaveIndex],
                            biomes[biomeX + 1, biomeY + 1].OctaveAmplitudes[octaveIndex],
                            horizontalT + offset.X, verticalT + offset.Y);
                        _heights[i, j] += octaveIndex == 0 ? biomeFactor : Mathf.PerlinNoise(i * octaveScale, j * octaveScale) * biomeFactor;
                    }

                    /*_heights[i, j] = 0.47f + 0.53f * biomeFactor * Mathf.PerlinNoise(i / 50f, j / 50f) *
                        Mathf.PerlinNoise(i / 500f, j / 500f) * Mathf.PerlinNoise(i / 100f, j / 100f);*/
                }
            }

            terrain.terrainData.SetHeights(0, 0, _heights);
            _canBuild = new bool[_width, _length];
            _horizontalRightFreeSpace = new int[_width, _length];
            _verticalFreeSpace = new int[_width, _length];

            var alphaMaps = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, 2];
            for (var i = 1; i < _width - 1; i++)
            for (var j = 1; j < _length - 1; j++)
            {
                var currentHeight = _heights[i, j];
                _canBuild[i, j] = currentHeight * terrainData.size.y > -_yOffset &&
                                  Mathf.Abs(currentHeight - _heights[i - 1, j]) * terrainData.size.y <
                                  maxHeightDifference &&
                                  Mathf.Abs(currentHeight - _heights[i + 1, j]) * terrainData.size.y <
                                  maxHeightDifference &&
                                  Mathf.Abs(currentHeight - _heights[i, j + 1]) * terrainData.size.y <
                                  maxHeightDifference &&
                                  Mathf.Abs(currentHeight - _heights[i, j - 1]) * terrainData.size.y <
                                  maxHeightDifference;

                if (_canBuild[i, j])
                {
                    alphaMaps[i, j, 0] = 1;
                    alphaMaps[i, j, 1] = 1;
                }
                else
                {
                    alphaMaps[i, j, 0] = 0;
                    alphaMaps[i, j, 1] = 1;
                }
            }

            for (var i = _width - 2; i > 0; i--)
            for (var j = _length - 2; j > 0; j--)
            {
                if (!_canBuild[i, j])
                    continue;
                _horizontalRightFreeSpace[i, j] = _horizontalRightFreeSpace[i + 1, j] + 1;
                _verticalFreeSpace[i, j] = _verticalFreeSpace[i, j + 1] + 1;
            }

            terrain.terrainData.SetAlphamaps(0, 0, alphaMaps);
        }

        public Vector3 GetWorldPosition(Vector2Int position)
        {
            var terrainData = terrain.terrainData;
            return new Vector3(
                position.y * terrainData.size.x / terrainData.heightmapResolution,
                _heights[position.x, position.y] * terrainData.size.y + _yOffset,
                position.x * terrainData.size.z / terrainData.heightmapResolution);
        }

        public void Paint(Vector2Int start, int size)
        {
            var alphaMaps = new float[size, size, 2];
            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                alphaMaps[i, j, 0] = 0;
                alphaMaps[i, j, 1] = 1;
            }

            terrain.terrainData.SetAlphamaps(start.y, start.x, alphaMaps);
        }

        public bool IsSquareFree(Vector2Int start, int size)
        {
            for (var i = 0; i < size; i++)
                if (_verticalFreeSpace[start.x + i, start.y] <
                    size) // || _horizontalFreeSpace[start.x + i, start.y] < size)
                    return false;
            return true;
        }

        public void ClaimSquare(Vector2Int start, int size)
        {
            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                if (!_canBuild[i + start.x, j + start.y])
                    Debug.LogError("eee");
                _canBuild[i + start.x, j + start.y] = false;
                _horizontalRightFreeSpace[i + start.x, j + start.y] = 0;
                _verticalFreeSpace[i + start.x, j + start.y] = 0;
            }

            for (var i = 0; i < size; i++)
            for (var j = 1; _canBuild[start.x - j, start.y + i]; j++)
            {
                _horizontalRightFreeSpace[start.x - j, start.y + i] = j;
            }

            for (var i = 0; i < size; i++)
            for (var j = 1; _canBuild[start.x + i, start.y - j]; j++)
            {
                _verticalFreeSpace[start.x + i, start.y - j] = j;
            }

            Paint(start, size);
        }

        public int GetTotalFreeSpace(Vector2Int position)
        {
            return _verticalFreeSpace[position.x, position.y] + _horizontalRightFreeSpace[position.x, position.y];
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
    }
}