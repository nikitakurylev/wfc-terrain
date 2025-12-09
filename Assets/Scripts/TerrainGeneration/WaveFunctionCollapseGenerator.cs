using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerrainGeneration
{
    public class WaveFunctionCollapseGenerator : MonoBehaviour
    {
        [SerializeField] private TerrainGenerationSettings _terrainGenerationSettings;

        public Biome[,] Generate(int size)
        {
            for (var i = 0; i < 100; i++)
            {
                Debug.Log($"WFC Attempt {i + 1}");

                if (TryGenerate(size, out var result))
                {
                    return result;
                }
            }

            Debug.LogError("WFC failed");
            return null;
        }

        private bool TryGenerate(int size, out Biome[,] result)
        {
            var array = new Biome[size, size];

            var positions = new List<(int, int)>(size * size);
            for (var i = 0; i < size * size; i++)
                positions.Add((i / size, i % size));
            
            Shuffle(positions);

            for (var n = 0; n < size * size; n++)
            {
                var bestPossibilities = new Dictionary<Biome, float>();
                var bestPossibilitiesCount = 0;
                int bestI = 0, bestJ = 0;
                foreach (var position in positions)
                {
                    var (i, j) = position;
                    var scores = _terrainGenerationSettings.Biomes.ToDictionary(b => b, _ => 1f);
                    for (var k = Mathf.Max(i - 1, 0); k < Mathf.Min(i + 2, size); k++)
                    for (var l = Mathf.Max(j - 1, 0); l < Mathf.Min(j + 2, size); l++)
                    {
                        if (array[k, l] == null)
                            continue;
                        
                        scores[array[k, l]] += array[k, l].ClusterFactor;
                    }
                    
                    for (var k = Mathf.Max(i - 1, 0); k < Mathf.Min(i + 2, size); k++)
                    for (var l = Mathf.Max(j - 1, 0); l < Mathf.Min(j + 2, size); l++)
                    {
                        if (array[k, l] == null)
                            continue;

                        foreach (var biome in _terrainGenerationSettings.Biomes
                                     .Where(biome => !array[k, l].Neighbours.Contains(biome)))
                        {
                            scores.Remove(biome);
                        }
                    }

                    if (!scores.Any())
                        continue;

                    if (bestPossibilitiesCount > 0 && bestPossibilitiesCount <= scores.Count)
                        continue;
                    
                    bestPossibilitiesCount = scores.Count;
                    bestPossibilities = scores;
                    bestI = i;
                    bestJ = j;
                }

                if (!bestPossibilities.Any())
                {
                    Debug.LogError($"Failed on step {n}");
                    result = null;
                    return false;
                }

                var scoreSum = bestPossibilities.Values.Sum();

                var random = Random.Range(0f, scoreSum);
                var currentSum = 0f;

                var possibility = bestPossibilities.Keys.First();

                foreach (var bestPossibility in bestPossibilities)
                {
                    currentSum += bestPossibility.Value;
                    if (!(random <= currentSum)) continue;
                    possibility = bestPossibility.Key;
                    break;
                }

                array[bestI, bestJ] = possibility;
                positions.Remove((bestI, bestJ));
            }

            result = array;
            return true;
        }
    
        private static void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}