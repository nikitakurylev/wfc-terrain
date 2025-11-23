using System;
using UnityEngine;

[Serializable]
public class VillagerNeed
{
    [field:SerializeField] public Need Need { get; set; }
    [field:SerializeField] public int Value { get; set; }
}