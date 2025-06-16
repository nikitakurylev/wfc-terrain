using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Building", menuName = "Village Generation/Building", order = 1)]
    public class Building : ScriptableObject
    {
        [field: SerializeField] public List<VillagerNeed> Resources { get; set; } = new();
        [field: SerializeField] public GameObject Prefab { private get; set; }
        [field: SerializeField] public int Size { get; private set; }

        public VillagerNeedsList ResourcesList { get; private set; }

        public VillagerNeedsList GetResourcesInstance() => new(ResourcesList);

        private void OnEnable()
        {
            ResourcesList = new VillagerNeedsList(Resources);
        }

        public virtual void SpawnBuilding(TerrainManager terrainManager, Vector2Int position)
        {
            Instantiate(Prefab, terrainManager.GetWorldPosition(position) + new Vector3(Size / 2f, 0, Size / 2f), Quaternion.identity);
        }
    }
}