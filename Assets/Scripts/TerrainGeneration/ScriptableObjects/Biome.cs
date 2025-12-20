using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Biome", menuName = "Terrain Generation/Biome", order = 1)]
    public class Biome : ScriptableObject
    {
        [field: SerializeField] public List<float> OctaveAmplitudes { get; private set; }
        [field: SerializeField] public List<Biome> Neighbours { get; private set; }
        [field: SerializeField] public float ClusterFactor { get; private set; }
        [field: SerializeField] private LayerHeight _color1;
        [field: SerializeField] private LayerHeight _color2;

        public int Color1 => _color1.LayerIndex;
        public int Color2 => _color2.LayerIndex;
        
        public float GetColorInterpolation(float height)
        {
            return Mathf.InverseLerp(_color1.Height, _color2.Height, height);
        }
    }

    [Serializable]
    public class LayerHeight
    {
        [field: SerializeField] public float Height { get; private set; }
        [field: SerializeField] public int LayerIndex { get; private set; }
    }
}