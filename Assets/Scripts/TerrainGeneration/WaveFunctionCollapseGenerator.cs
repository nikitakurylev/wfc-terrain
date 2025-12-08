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
                var bestPossibilities = new List<Biome>();
                var bestPossibilitiesCount = 0;
                int bestI = 0, bestJ = 0, bestScore = 0;
                foreach (var position in positions)
                {
                    var (i, j) = position;
                    var possibilities = _terrainGenerationSettings.Biomes.ToList();

                    var scores = _terrainGenerationSettings.Biomes.ToDictionary(b => b, _ => 0);
                    for (var k = Mathf.Max(i - 1, 0); k < Mathf.Min(i + 2, size); k++)
                    for (var l = Mathf.Max(j - 1, 0); l < Mathf.Min(j + 2, size); l++)
                    {
                        if (array[k, l] == null)
                            continue;
                        
                        foreach (var neighbour in array[k, l].Neighbours)
                            scores[neighbour]++;
                    }
                    
                    for (var k = Mathf.Max(i - 1, 0); k < Mathf.Min(i + 2, size); k++)
                    for (var l = Mathf.Max(j - 1, 0); l < Mathf.Min(j + 2, size); l++)
                    {
                        if (array[k, l] == null)
                            continue;

                        foreach (var biome in _terrainGenerationSettings.Biomes
                                     .Where(biome => !array[k, l].Neighbours.Contains(biome)))
                        {
                            possibilities.Remove(biome);
                        }
                    }

                    if (!possibilities.Any())
                        continue;

                    if (bestPossibilitiesCount > 0 && bestPossibilitiesCount < possibilities.Count)
                        continue;
                    
                    var maxScore = possibilities.Max(b => scores[b]);

                    if (bestPossibilitiesCount == possibilities.Count && bestScore > maxScore)
                        continue;
                    
                    bestPossibilitiesCount = possibilities.Count;
                    bestScore = maxScore;
                    bestPossibilities = possibilities.Where(b => scores[b] == maxScore).ToList();
                    bestI = i;
                    bestJ = j;
                }

                if (!bestPossibilities.Any())
                {
                    Debug.LogError($"Failed on step {n}");
                    result = null;
                    return false;
                }

                var possibility = bestPossibilities[Random.Range(0, bestPossibilities.Count)];

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