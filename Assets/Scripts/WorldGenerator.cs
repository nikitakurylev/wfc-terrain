using TerrainGeneration;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;
    [SerializeField] private VillageGenerator villageGenerator;

    private void Start()
    {
        terrainManager.GenerateTerrain();
        //villageGenerator.GenerateVillage();
    }
}