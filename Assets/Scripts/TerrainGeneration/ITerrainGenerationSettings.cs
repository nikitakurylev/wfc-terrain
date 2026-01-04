using System.Collections.Generic;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public interface ITerrainGenerationSettings
    {
        public int Size { get; }
        public int BiomeSize { get; }
        public List<Biome> Biomes { get; }
        public AnimationCurve InterpolationCurve { get; }
        public List<float> OctaveScales { get; }
    }
}