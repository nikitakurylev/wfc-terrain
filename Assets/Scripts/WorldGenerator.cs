using TerrainGeneration;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private TerrainManager terrainManager;
    [InspectorButton("OnButtonClicked")]
    public bool generate;

    private void OnButtonClicked()
    {
        terrainManager.GenerateTerrain();
    }
    
    private void Start()
    {
        terrainManager.GenerateTerrain();
    }
}