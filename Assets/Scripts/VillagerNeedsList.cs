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
    
    public VillagerNeedsList(IEnumerable<VillagerNeed> needs)
    {
        _needsDictionary = needs.ToDictionary(n => n.Need, n => n.Value);
    }

    public VillagerNeedsList(VillagerNeedsList needs)
    {
        _needsDictionary = needs._needsDictionary.ToDictionary(n => n.Key, n => n.Value);
    }
    
    public void IncreaseNeeds(IEnumerable<VillagerNeed> needs)
    {
        foreach (var need in needs.Where(need => !_needsDictionary.TryAdd(need.Need, need.Value)))
            _needsDictionary[need.Need] += need.Value;
    }

    public bool IsSatisfied() => _needsDictionary.All(n => n.Value == 0);

    public float GetSatisfactionPotential(VillagerNeedsList satisfactionSource)
    {
        var satisfiedNeeds = 0;
        var potential = _needsDictionary.Sum(need =>
        {
            if (!satisfactionSource._needsDictionary.TryGetValue(need.Key, out var resource))
                return 0f;

            satisfiedNeeds++;
            float distance = Mathf.Abs(resource - need.Value);

            return 1f - Mathf.Min(distance / need.Value, 1f);
        });
        return potential * satisfiedNeeds;
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