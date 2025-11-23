using System.Linq;
using TerrainGeneration.ScriptableObjects;
using UnityEngine;

namespace TerrainGeneration
{
    public class WaveFunctionCollapseGenerator : MonoBehaviour
    {
        [SerializeField] private TerrainGenerationSettings _terrainGenerationSettings;

        public Biome[,] Generate(int size)
        {
            var debug = string.Empty;
            var array = new Biome[size, size];
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var scores = _terrainGenerationSettings.Biomes.ToDictionary(b => b, _ => 0);
                    for (var k = Mathf.Max(i - 1, 0); k < Mathf.Min(i + 1, size); k++)
                    for (var l = Mathf.Max(j - 1, 0); l < Mathf.Min(j + 1, size); l++)
                    {
                        if (array[k, l] == null)
                            continue;
                        foreach (var neighbour in array[k, l].Neighbours)
                            scores[neighbour]++;
                    }

                    var maxScore = scores.Values.Max();

                    var possibilities = scores
                        .Where(s => s.Value == maxScore)
                        .Select(s => s.Key)
                        .ToList();

                    array[i, j] = possibilities[Random.Range(0, possibilities.Count)];
                    debug += $"{array[i, j].name} ";
                }

                debug += "\n";
            }

            Debug.LogWarning(debug);

            return array;
        }
    }
}