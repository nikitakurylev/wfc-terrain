using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private float maxHeightDifference;

    private bool[,] _canBuild;
    private int[,] _horizontalFreeSpace;
    private int[,] _verticalFreeSpace;
    private float[,] _heights;
    private float _yOffset;
    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GenerateTerrain()
    {
        _yOffset = terrain.transform.position.y;
        
        _heights = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution,
            terrain.terrainData.heightmapResolution);

        var width = _heights.GetLength(0);
        var length = _heights.GetLength(1);

        for (var i = 0; i < width; i++)
        for (var j = 0; j < length; j++)
            _heights[i, j] = 0.47f + 0.53f * Mathf.PerlinNoise(i / 50f, j / 50f) *
                Mathf.PerlinNoise(i / 500f, j / 500f) * Mathf.PerlinNoise(i / 100f, j / 100f);

        terrain.terrainData.SetHeights(0, 0, _heights);
        _canBuild = new bool[width, length];
        _horizontalFreeSpace = new int[width, length];
        _verticalFreeSpace = new int[width, length];

        var alphaMaps = new float[terrain.terrainData.alphamapResolution, terrain.terrainData.alphamapResolution, 2];
        for (var i = 1; i < width - 1; i++)
        for (var j = 1; j < length - 1; j++)
        {
            var currentHeight = _heights[i, j];
            _canBuild[i, j] = currentHeight > 0.5f &&
                              Mathf.Abs(currentHeight - _heights[i - 1, j]) < maxHeightDifference &&
                              Mathf.Abs(currentHeight - _heights[i + 1, j]) < maxHeightDifference &&
                              Mathf.Abs(currentHeight - _heights[i, j + 1]) < maxHeightDifference &&
                              Mathf.Abs(currentHeight - _heights[i, j - 1]) < maxHeightDifference;

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
        return new Vector3(
            position.y * terrain.terrainData.size.x / terrain.terrainData.heightmapResolution,
            _heights[position.x, position.y] * terrain.terrainData.size.y + _yOffset,
            position.x * terrain.terrainData.size.z / terrain.terrainData.heightmapResolution);
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
            _horizontalFreeSpace[start.x + i, start.y - j] = j;
        }
        
    }
}