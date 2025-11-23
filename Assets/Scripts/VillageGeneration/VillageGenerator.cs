using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using TerrainGeneration;
using UnityEngine;
using Utils;

public class VillageGenerator : MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private VillageGenerationSettings villageGenerationSettings;
    private Vector2Int _villageCenter = new(256, 256);

    public void GenerateVillage()
    {
        var villageNeeds = new VillagerNeedsList();
        foreach (var villager in villageGenerationSettings.VillagersList.Villagers)
            villageNeeds.IncreaseNeeds(villager.Needs);

        var buildingPositions = new List<Vector2Int>();

        var startTime = Time.realtimeSinceStartup;
        while (!villageNeeds.IsSatisfied())
        {
            var bestBuilding =
                villageGenerationSettings.BuildingsList.Buildings.MaxBy(v =>
                {
                    var potential = villageNeeds.GetSatisfactionPotential(v.ResourcesList);
                    Debug.Log($"Potential of {v.name} is {potential}");
                    return potential;
                });

            Debug.Log($"Chose {bestBuilding.name}");

            var startPos = FindClosestFree(_villageCenter, bestBuilding.Size);
            if (!startPos.HasValue)
                throw new Exception("Couldn't find a free space!");
            bestBuilding.SpawnBuilding(terrainManager, startPos.Value);
            buildingPositions.Add(startPos.Value);

            _villageCenter = buildingPositions.Aggregate(new Vector2Int(0, 0), (s, v) => s + v) /
                            buildingPositions.Count;

            var buildingResources = bestBuilding.GetResourcesInstance();

            villageNeeds.Satisfy(buildingResources);
        }
        
        Debug.Log($"Generation took {Time.realtimeSinceStartup - startTime} seconds");
    }

    private Vector2Int? FindClosestFree(Vector2Int start, int size)
    {
        var visited = new List<Vector2Int>();
        var positions = new PriorityQueue<Vector2Int, int>();
        positions.Enqueue(start, 0);
        while (positions.TryDequeue(out var current, out var priority))
        {
            if (visited.Any(v => v == current))
                continue;
            visited.Add(current);
            if (terrainManager.IsSquareFree(current, size))
            {
                terrainManager.ClaimSquare(current, size);
                return current;
            }

            EnqueueIfInBounds(positions, current + Vector2Int.left);
            EnqueueIfInBounds(positions, current + Vector2Int.right);
            EnqueueIfInBounds(positions, current + Vector2Int.down);
            EnqueueIfInBounds(positions, current + Vector2Int.up);
        }

        return null;
    }

    private void EnqueueIfInBounds(PriorityQueue<Vector2Int, int> queue, Vector2Int position)
    {
        if (terrainManager.IsInBounds(position))
            queue.Enqueue(position, Mathf.Abs(position.x - _villageCenter.x) + Mathf.Abs(position.y - _villageCenter.y) - terrainManager.GetTotalFreeSpace(position));
    }

    public int Compare(Vector2Int x, Vector2Int y)
    {
        throw new NotImplementedException();
    }
}