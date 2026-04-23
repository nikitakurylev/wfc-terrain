using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public interface ITerrainInterpolator
    {
        (Biome, float)[] ComputeWeights(Vector2Int position);
    }
}