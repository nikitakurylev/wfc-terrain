using System.Collections.Generic;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public interface ITerrainInterpolator
    {
        Dictionary<Biome, float> ComputeWeights(Vector2Int position);
    }
}