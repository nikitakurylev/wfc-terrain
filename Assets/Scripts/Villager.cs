using System.Collections.Generic;

public class Villager
{
    public VillagerNeedsList Needs { get; private set; }

    public Villager(List<VillagerNeed> needs)
    {
        Needs = new VillagerNeedsList(needs);
    }

    public bool IsSatisfied() => Needs.IsSatisfied();
}