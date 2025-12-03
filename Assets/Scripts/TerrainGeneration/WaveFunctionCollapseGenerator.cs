using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TerrainGeneration.ScriptableObjects;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TerrainGeneration
{
    public class WaveFunctionCollapseGenerator : MonoBehaviour
    {
        [SerializeField] private TerrainGenerationSettings _terrainGenerationSettings;

        public Biome[,] Generate(int size)
        {
            if (!TryGenerate(size, out var result))
            {
                Debug.LogError("WFC failed");
            }

            return result;
        }

        private bool TryGenerate(int size, out Biome[,] result)
        {
            var array = new Biome[size, size];
            for (var i = 0; i < size; i++)
            {
                int j;
                for (j = 0; j < size && j >= 0; j++)
                {
                    Debug.Log($"{i} {j}");
                    var rollbackValue = array[i, j];
                    array[i, j] = null;

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
                            scores.Remove(biome);
                        }
                    }
                    
                    if (!scores.Any())
                    {
                        Debug.LogError("Rolling Back");
                        j -= 2;
                        continue;
                    }

                    var maxScore = scores.Values.Max();

                    var possibilities = scores
                        .Where(s => s.Value == maxScore)
                        .Select(s => s.Key)
                        .ToList();

                    var possibility = possibilities[Random.Range(0, possibilities.Count)];

                    if (possibility == rollbackValue)
                    {
                        Debug.LogError("Rolling Back");
                        j -= 2;
                        continue;
                    }
                    
                    array[i, j] = possibility;
                }

                if (j >= 0) continue;
                
                i -= 2;
                
                if (i >= 0) continue;

                result = null;
                return false;
            }

            result = array;
            return true;
        }
    }
}