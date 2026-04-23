using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration.Interpolators
{
    public class LinearInterpolator : ITerrainInterpolator
    {
        private readonly Biome[,] _biomes;
        private readonly int _biomeSize;
        private readonly AnimationCurve _interpolationCurve;

        public LinearInterpolator(Biome[,] biomes, int biomeSize, AnimationCurve interpolationCurve)
        {
            _biomes = biomes;
            _biomeSize = biomeSize;
            _interpolationCurve = interpolationCurve;
        }
        public (Biome, float)[] ComputeWeights(Vector2Int position)
        {
            var horizontalT = (position.x - 1) % _biomeSize / (float)_biomeSize;
            var biomeX = (position.x - 1) / _biomeSize;
            var verticalT = (position.y - 1) % _biomeSize / (float)_biomeSize;
            var biomeY = (position.y - 1) / _biomeSize; 
            
            var topLeft = _biomes[biomeX, biomeY];
            var topRight = _biomes[biomeX + 1, biomeY];
            var bottomLeft = _biomes[biomeX, biomeY + 1];
            var bottomRight = _biomes[biomeX + 1, biomeY + 1];

            var lerp = BiLerp(horizontalT, verticalT);

            return new[]
            {
                (topLeft, lerp.x),
                (topRight, lerp.y),
                (bottomLeft, lerp.z),
                (bottomRight, lerp.w)
            };
        }

        private Vector4 BiLerp(float horizontalT, float verticalT)
        {
            return Vector4.Lerp(
                Vector4.Lerp(new Vector4(1, 0, 0, 0),
                    new Vector4(0, 1, 0, 0),
                    _interpolationCurve
                        .Evaluate(horizontalT)),
                Vector4.Lerp(new Vector4(0, 0, 1, 0),
                    new Vector4(0, 0, 0, 1),
                    _interpolationCurve
                        .Evaluate(horizontalT)),
                _interpolationCurve.Evaluate(verticalT));
        }
    }
}