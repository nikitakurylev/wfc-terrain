using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Villagers List", menuName = "Village Generation/Villagers List", order = 1)]
    public class VillagersList : ScriptableObject
    {
        [field: SerializeField] public List<VillagerPrototype> Villagers { get; set; }
    }
}