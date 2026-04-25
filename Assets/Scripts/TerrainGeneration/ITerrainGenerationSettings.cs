using System.Collections.Generic;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public interface ITerrainGenerationSettings
    {
        int Size { get; }
        int BiomeSize { get; }
        List<Biome> Biomes { get; }
        List<float> OctaveScales { get; }
        InterpolatorType InterpolatorType { get; }
    }
}