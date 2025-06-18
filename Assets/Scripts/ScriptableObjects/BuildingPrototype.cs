using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Building", menuName = "Village Generation/Building", order = 1)]
    public class BuildingPrototype : ScriptableObject
    {
        [field: SerializeField] private List<VillagerNeed> Resources { get; set; } = new();
        [field: SerializeField] private GameObject Prefab { get; set; }
        [field: SerializeField] public int Size { get; private set; }

        public VillagerNeedsList ResourcesList { get; private set; }

        public VillagerNeedsList GetResourcesInstance() => new(ResourcesList);

        private void OnEnable()
        {
            ResourcesList = new VillagerNeedsList(Resources);
        }

        public virtual void SpawnBuilding(TerrainManager terrainManager, Vector2Int position)
        {
            var building = Instantiate(Prefab, terrainManager.GetWorldPosition(position) + new Vector3(Size, 0, Size) / 2f * terrainManager.TileScale, Quaternion.identity);
            building.transform.localScale *= terrainManager.TileScale;
        }
    }
}