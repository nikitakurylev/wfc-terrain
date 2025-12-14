using System.Collections.Generic;
using UnityEngine;

namespace TerrainGeneration
{
    public class CachedPerlinNoise
    {
        private int _i, _j;
        private Dictionary<float, float> _octaveValue = new();

        public CachedPerlinNoise(int i, int j)
        {
            _i = i;
            _j = j;
        }

        public float GetValue(float octave)
        {
            if (_octaveValue.TryGetValue(octave, out var value))
                return value;
            _octaveValue[octave] = Mathf.PerlinNoise(_i * octave, _j * octave);
            return _octaveValue[octave];
        }
    }
}