using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillTreeData
{
    public List<SkillNodeData> nodes;

    public void CheckAllUnlocked()
    {
        foreach (SkillNodeData data in nodes)
        {
            data.CheckUnlocked();
        }
    }

    public void LoadAllUnlockedAbilities()
    {
        foreach (SkillNodeData data in nodes)
        {
            data.LoadUnlockedAbilities();
        }
    }

    public void GetAllUnlockedAbilities()
    {
        List<Ability> abilities = new List<Ability>();
        foreach (SkillNodeData data in nodes)
        {
            abilities.AddRange(data.unlockedAbilties);
        }
    }
}

[System.Serializable]
public class SkillNodeData
{
    public string id;
    public bool unlocked;
    public bool learned;
    public int cost;
    public List<SkillNodeData> requirements;
    public List<string> unlocks;
    public List<Ability> unlockedAbilties;

    public void CheckUnlocked()
    {
        foreach (SkillNodeData data in requirements)
        {
            if (!data.learned)
            {
                unlocked = false;
                return;
            }
        }
        unlocked = true;
    }

    public void LoadUnlockedAbilities()
    {
        foreach(string abilityName in unlocks)
        {
            unlockedAbilties.Add(Resources.Load(abilityName) as Ability);
        }
    }
}