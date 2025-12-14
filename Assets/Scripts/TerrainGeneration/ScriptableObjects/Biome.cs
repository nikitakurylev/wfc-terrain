using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TerrainGeneration.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Biome", menuName = "Terrain Generation/Biome", order = 1)]
    public class Biome : ScriptableObject
    {
        [field: SerializeField] public List<float> OctaveAmplitudes { get; private set; }
        [field: SerializeField] public List<Biome> Neighbours { get; private set; }
        [field: SerializeField] public float ClusterFactor { get; private set; }
        [field: SerializeField] private List<LayerHeight> _colors;

        private List<LayerValue>[] _cachedColors = new List<LayerValue>[1024];

        private int QuantizeFloat(float value) => Mathf.RoundToInt(1023 * value);
        
        public List<LayerValue> GetLayers(float height)
        {
            var index = QuantizeFloat(height);
            if (_cachedColors[index] != null)
            {
                return _cachedColors[index];
            }

            var colorIndex = _colors.FindIndex(color => height >= color.Height);
            if (colorIndex == -1)
            {
                _cachedColors[index] = new List<LayerValue> { new() { Layer = _colors.Last().Layer, Value = 1f } };
                return _cachedColors[index];
            }
            if (colorIndex == 0)
            {
                _cachedColors[index] = new List<LayerValue> { new() { Layer = _colors[colorIndex].Layer, Value = 1f } };
                return _cachedColors[index];
            }

            var t = Mathf.Clamp01((height - _colors[colorIndex].Height) / (_colors[colorIndex - 1].Height - _colors[colorIndex].Height));

            _cachedColors[index] = new List<LayerValue>
            {
                new() { Layer = _colors[colorIndex - 1].Layer, Value = t },
                new() { Layer = _colors[colorIndex].Layer, Value = 1f - t }
            };

            return _cachedColors[index];
        }
    }

    [Serializable]
    public class LayerHeight
    {
        [field: SerializeField] public float Height { get; private set; }
        [field: SerializeField] public TerrainLayer Layer { get; private set; }
    }

    public class LayerValue
    {
        public TerrainLayer Layer { get; set; }
        public float Value { get; set; }
    }
}