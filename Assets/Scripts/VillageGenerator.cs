using System;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
using UnityEngine;

public class VillageGenerator : MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private VillageGenerationSettings villageGenerationSettings;

    public void GenerateVillage()
    {
        var villageNeeds = new VillagerNeedsList();
        foreach (var villager in villageGenerationSettings.VillagersList.Villagers)
            villageNeeds.IncreaseNeeds(villager.Needs);

        var villageCenter = new Vector2Int(256, 256);
        var buildingPositions = new List<Vector2Int>();

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

            var startPos = FindClosestFree(villageCenter, bestBuilding.Size);
            if (!startPos.HasValue)
                throw new Exception("Couldn't find a free space!");
            bestBuilding.SpawnBuilding(terrainManager, startPos.Value);
            buildingPositions.Add(startPos.Value);

            villageCenter = buildingPositions.Aggregate(new Vector2Int(0, 0), (s, v) => s + v) /
                            buildingPositions.Count;

            var buildingResources = bestBuilding.GetResourcesInstance();

            villageNeeds.Satisfy(buildingResources);
        }
    }

    private Vector2Int? FindClosestFree(Vector2Int start, int size)
    {
        var visited = new List<Vector2Int>();
        var positions = new Queue<Vector2Int>();
        positions.Enqueue(start);
        while (positions.Any())
        {
            var current = positions.Dequeue();
            if (visited.Any(v => v == current))
                continue;
            visited.Add(current);
            if (terrainManager.IsSquareFree(current, size))
            {
                terrainManager.ClaimSquare(current, size);
                return current;
            }

            positions.Enqueue(current + Vector2Int.left);
            positions.Enqueue(current + Vector2Int.right);
            positions.Enqueue(current + Vector2Int.down);
            positions.Enqueue(current + Vector2Int.up);
        }

        return null;
    }
}