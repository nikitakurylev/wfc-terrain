using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Villager", menuName = "Village Generation/Villager", order = 1)]
    public class VillagerPrototype : ScriptableObject
    {
        [field: SerializeField] public List<VillagerNeed> Needs { get; set; }
    }
}