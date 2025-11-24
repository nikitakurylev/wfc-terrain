using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Terrain Generation Settings", menuName = "Terrain Generation/Terrain Generation Settings", order = 1)]
    public class TerrainGenerationSettings : ScriptableObject
    {
        [field:SerializeField] public int BiomeSize { get; private set; }
        [field:SerializeField] public List<Biome> Biomes { get; private set; }
        [field:SerializeField] public AnimationCurve InterpolationCurve { get; private set; }
    }
}