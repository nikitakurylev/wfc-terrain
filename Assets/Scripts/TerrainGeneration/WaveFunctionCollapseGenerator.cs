using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TerrainGeneration.ScriptableObjects;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TerrainGeneration
{
    public class WaveFunctionCollapseGenerator : MonoBehaviour
    {
        [SerializeField] private TerrainGenerationSettings _terrainGenerationSettings;

        public Biome[,] GenerateBiomes(int size)
        {
            var result = new Biome[size, size];

            return GenerateBiomes(result) ? result : null;
        }
        
        public bool GenerateBiomes(Biome[,] result)
        {
            for (var i = 0; i < 100; i++)
            {
                if (TryGenerateBiomes(result))
                    return true;
            }

            Debug.LogError("WFC failed");
            return false;
        }

        private bool TryGenerateBiomes(Biome[,] result)
        {
            var size = result.GetLength(0);
            var possibilites = new HashSet<Biome>[size, size];
            
            var positions = new List<(int, int)>(size * size);

            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
            {
                if (result[i, j] == null)
                {
                    possibilites[i, j] = new HashSet<Biome>(_terrainGenerationSettings.Biomes);
                    positions.Add((i, j));
                    
                    for (var k = Mathf.Max(i - 1, 0); k < Mathf.Min(i + 2, size); k++)
                    for (var l = Mathf.Max(j - 1, 0); l < Mathf.Min(j + 2, size); l++)
                    {
                        if (result[k, l] == null)
                            continue;
                        possibilites[i, j].RemoveWhere(p => !p.Neighbours.Contains(result[k, l]));
                    }
                }
                else
                {
                    possibilites[i, j] = new HashSet<Biome>() { result[i, j] };
                }
            }

            Shuffle(positions);

            while (positions.Any())
            {
                int bestI = -1, bestJ = -1;
                foreach (var position in positions)
                {
                    var (i, j) = position;

                    if (possibilites[i, j].Count == 1)
                        continue;

                    if (bestI >= 0 && bestJ >= 0 && possibilites[bestI, bestJ].Count <= possibilites[i, j].Count)
                        continue;

                    bestI = i;
                    bestJ = j;
                }

                if (bestI == -1)
                {
                    if (positions.All(p => possibilites[p.Item1, p.Item2].Count == 1))
                        break;
                }

                if (!possibilites[bestI, bestJ].Any())
                {
                    Debug.LogError($"Failed with {positions.Count} tiles left");
                    result = null;
                    return false;
                }

                var scores = possibilites[bestI, bestJ].ToDictionary(b => b, _ => 1f);
                for (var k = Mathf.Max(bestI - 1, 0); k < Mathf.Min(bestI + 2, size); k++)
                for (var l = Mathf.Max(bestJ - 1, 0); l < Mathf.Min(bestJ + 2, size); l++)
                {
                    var otherBiome = possibilites[k, l].First();
                    if (possibilites[k, l].Count > 1 || !scores.ContainsKey(otherBiome))
                        continue;

                    scores[otherBiome] += otherBiome.ClusterFactor;
                }

                var scoreSum = scores.Values.Sum();

                var random = Random.Range(0f, scoreSum);
                var currentSum = 0f;

                var possibility = possibilites[bestI, bestJ].First();

                foreach (var bestPossibility in scores)
                {
                    currentSum += bestPossibility.Value;
                    if (!(random <= currentSum)) continue;
                    possibility = bestPossibility.Key;
                    break;
                }

                possibilites[bestI, bestJ].RemoveWhere(p => p != possibility);
                positions.Remove((bestI, bestJ));

                for (var k = Mathf.Max(bestI - 1, 0); k < Mathf.Min(bestI + 2, size); k++)
                for (var l = Mathf.Max(bestJ - 1, 0); l < Mathf.Min(bestJ + 2, size); l++)
                {
                    if (possibilites[k, l].Count == 1)
                        continue;
                    possibilites[k, l].RemoveWhere(p => !p.Neighbours.Contains(possibility));
                    if (possibilites[k, l].Count == 1)
                        positions.Remove((k, l));
                }
            }

            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                result[i, j] = possibilites[i, j].First();

            return true;
        }

        private static void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}