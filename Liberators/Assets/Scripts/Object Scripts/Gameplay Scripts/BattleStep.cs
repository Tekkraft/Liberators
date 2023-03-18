using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStep
{
    Dictionary<GameObject, List<BattleDetail>> details = new Dictionary<GameObject, List<BattleDetail>>(); 

    public BattleStep() { }

    public void AddDetail(GameObject unit, BattleDetail detail)
    {
        if (details.ContainsKey(unit))
        {
            details[unit].Add(detail);
        }
        else
        {
            details.Add(unit, new List<BattleDetail>() { detail });
        }
    }

    public Dictionary<GameObject, List<BattleDetail>> GetBattleDetails()
    {
        return details;
    }
}

public class BattleDetail
{
    string effect;
    int value;
    bool dead;
    Status status;
    bool critical;

    public BattleDetail(string effect)
    {
        this.effect = effect;
    }

    public BattleDetail(int value, bool dead, bool critical)
    {
        effect = "damage";
        this.value = value;
        this.dead = dead;
        this.critical = critical;
    }

    public BattleDetail()
    {
        effect = "miss";
    }

    public BattleDetail(int value)
    {
        effect = "heal";
        this.value = value;
    }

    public BattleDetail(Status status)
    {
        this.status = status;
        effect = "status";
    }

    public string GetEffect()
    {
        return effect;
    }

    public int GetValue()
    {
        return value;
    }

    public bool GetDead()
    {
        return dead;
    }

    public bool GetCritical()
    {
        return critical;
    }

    public Status GetStatus()
    {
        return status;
    }
}