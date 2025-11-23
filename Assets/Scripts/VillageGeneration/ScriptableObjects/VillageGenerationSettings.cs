using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Village Generation Settings", menuName = "Village Generation/Village Generation Settings", order = 1)]
    public class VillageGenerationSettings : ScriptableObject
    {
        [field:SerializeField] public BuildingsList BuildingsList { get; set; }
        [field:SerializeField] public VillagersList VillagersList { get; set; }
    }
}