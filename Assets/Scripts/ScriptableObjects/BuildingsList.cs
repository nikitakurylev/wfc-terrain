using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Buildings List", menuName = "Village Generation/Buildings List", order = 1)]
    public class BuildingsList : ScriptableObject
    {
        [field: SerializeField] public List<Building> Buildings { get; set; }
    }
}