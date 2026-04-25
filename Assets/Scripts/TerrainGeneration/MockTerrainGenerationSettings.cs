using System.Collections.Generic;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public class MockTerrainGenerationSettings : ITerrainGenerationSettings
    {
        public int Size { get; set; }
        public int BiomeSize { get; set; }
        public List<Biome> Biomes { get; set; }
        public List<float> OctaveScales { get; set; }
        public InterpolatorType InterpolatorType { get; set; }
    }
}