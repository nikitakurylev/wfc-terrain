using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillagerNeedsList
{
    private readonly Dictionary<Need, int> _needsDictionary;

    public VillagerNeedsList()
    {
        _needsDictionary = new Dictionary<Need, int>();
    }
    
    public VillagerNeedsList(List<VillagerNeed> needs)
    {
        _needsDictionary = needs.ToDictionary(n => n.Need, n => n.Value);
    }

    public VillagerNeedsList(VillagerNeedsList needs)
    {
        _needsDictionary = needs._needsDictionary.ToDictionary(n => n.Key, n => n.Value);
    }
    
    public void IncreaseNeed(List<VillagerNeed> needs)
    {
        foreach (var need in needs)
            IncreaseNeed(need.Need, need.Value);
    }

    private void IncreaseNeed(Need need, int value)
    {
        if (!_needsDictionary.TryAdd(need, value))
            _needsDictionary[need] += value;
    }

    public bool IsSatisfied() => _needsDictionary.All(n => n.Value == 0);

    public int GetSatisfactionPotential(VillagerNeedsList satisfactionSource)
    {
        return _needsDictionary.Sum(need =>
        {
            satisfactionSource.EstimateSatisfactionIfPossible(need.Key, need.Value, out var satisfaction);
            return satisfaction;
        });
    }

    public void Satisfy(VillagerNeedsList satisfactionSource)
    {
        foreach (var needKey in _needsDictionary.Keys.ToList())
        {
            _needsDictionary[needKey] -= satisfactionSource.TakeNeed(needKey, _needsDictionary[needKey]);
            if (_needsDictionary[needKey] == 0)
                _needsDictionary.Remove(needKey);
        }
    }

    private int TakeNeed(Need need, int value)
    {
        if (!EstimateSatisfactionIfPossible(need, value, out var satisfaction))
            return 0;
        _needsDictionary[need] -= satisfaction;
        return satisfaction;
    }

    private bool EstimateSatisfactionIfPossible(Need need, int value, out int satisfaction)
    {
        satisfaction = 0;
        if (!_needsDictionary.TryGetValue(need, out var resource))
            return false;

        satisfaction = Mathf.Min(resource, value);

        return true;
    }
}