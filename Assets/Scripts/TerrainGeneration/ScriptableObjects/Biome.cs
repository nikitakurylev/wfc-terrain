using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Biome", menuName = "Terrain Generation/Biome", order = 1)]
    public class Biome : ScriptableObject
    {
        [field:SerializeField] public List<float> OctaveAmplitudes { get; private set; }
        [field:SerializeField] public List<Biome> Neighbours { get; private set; }
    }
}