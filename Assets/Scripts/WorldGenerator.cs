using TerrainGeneration;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;

    private void Start()
    {
        terrainManager.GenerateTerrain();
    }
}