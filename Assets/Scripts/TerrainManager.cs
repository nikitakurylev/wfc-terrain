using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private float maxHeightDifference;
    
    public float TileScale { get; private set; }

    private bool[,] _canBuild;
    private int[,] _horizontalFreeSpace;
    private int[,] _verticalFreeSpace;
    private float[,] _heights;
    private float _yOffset;
    
    public void GenerateTerrain()
    {
        _yOffset = terrain.transform.position.y;

        var terrainData = terrain.terrainData;
        _heights = terrain.terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
            terrainData.heightmapResolution);

        var width = _heights.GetLength(0);
        var length = _heights.GetLength(1);
        TileScale = terrainData.size.x / width;

        for (var i = 0; i < width; i++)
        for (var j = 0; j < length; j++)
            _heights[i, j] = 0.47f + 0.53f * Mathf.PerlinNoise(i / 50f, j / 50f) *
                Mathf.PerlinNoise(i / 500f, j / 500f) * Mathf.PerlinNoise(i / 100f, j / 100f);

        terrain.terrainData.SetHeights(0, 0, _heights);
        _canBuild = new bool[width, length];
        _horizontalFreeSpace = new int[width, length];
        _verticalFreeSpace = new int[width, length];

        var alphaMaps = new float[terrainData.alphamapResolution, terrainData.alphamapResolution, 2];
        for (var i = 1; i < width - 1; i++)
        for (var j = 1; j < length - 1; j++)
        {
            var currentHeight = _heights[i, j];
            _canBuild[i, j] = currentHeight * terrainData.size.y > -_yOffset &&
                              Mathf.Abs(currentHeight - _heights[i - 1, j]) * terrainData.size.y < maxHeightDifference &&
                              Mathf.Abs(currentHeight - _heights[i + 1, j]) * terrainData.size.y < maxHeightDifference &&
                              Mathf.Abs(currentHeight - _heights[i, j + 1]) * terrainData.size.y < maxHeightDifference &&
                              Mathf.Abs(currentHeight - _heights[i, j - 1]) * terrainData.size.y < maxHeightDifference;

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
        for (var i = width - 2; i > 0; i--)
        for (var j = length - 2; j > 0; j--)
        {
            if (!_canBuild[i, j])
                continue;
            _horizontalFreeSpace[i, j] = _horizontalFreeSpace[i + 1, j] + 1;
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
            if (_verticalFreeSpace[start.x + i, start.y] < size || _horizontalFreeSpace[start.x + i, start.y] < size)
                return false;
        return true;
    }

    public void ClaimSquare(Vector2Int start, int size)
    {
        for (var i = 0; i < size; i++)
        for (var j = 0; j < size; j++)
        {
            if(!_canBuild[i + start.x, j + start.y])
                Debug.LogError("eee");
            _canBuild[i + start.x, j + start.y] = false;
            _horizontalFreeSpace[i + start.x, j + start.y] = 0;
            _verticalFreeSpace[i + start.x, j + start.y] = 0;
        }

        for (var i = 0; i < size; i++)
        for (var j = 1; _canBuild[start.x - j, start.y + i]; j++)
        {
            _horizontalFreeSpace[start.x - j, start.y + i] = j;
        }

        for (var i = 0; i < size; i++)
        for (var j = 1; _canBuild[start.x + i, start.y - j]; j++)
        {
            _verticalFreeSpace[start.x + i, start.y - j] = j;
        }
        
        Paint(start, size);
    }
}