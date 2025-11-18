using System.Linq;
using UnityEngine;

public class WaveFunctionCollapseGenerator : MonoBehaviour
{
    private const int size = 10;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var debug = string.Empty;
        var array = new int[size, size];
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                var scores = new int[3];
                for (var k = Mathf.Max(i - 1, 0); k < Mathf.Min(i + 1, size); k++)
                for (var l = Mathf.Max(j - 1, 0); l < Mathf.Min(j + 1, size); l++)
                {
                    switch (array[k, l])
                    {
                        case 1:
                            scores[0]++;
                            scores[1]++;
                            break;
                        case 2:
                            scores[0]++;
                            scores[1]++;
                            scores[2]++;
                            break;
                        case 3:
                            scores[1]++;
                            scores[2]++;
                            break;
                    }
                }

                var maxScore = scores.Max();

                var possibilities = scores
                    .Select((value, index) => (value, index))
                    .Where(s => s.value == maxScore)
                    .Select(s => s.index)
                    .ToList();

                array[i, j] = possibilities[Random.Range(0, possibilities.Count)] + 1;
                debug += $"{array[i, j]} ";
            }

            debug += "\n";
        }
        
        Debug.LogWarning(debug);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
