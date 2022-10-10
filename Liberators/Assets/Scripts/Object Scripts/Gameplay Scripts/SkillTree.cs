using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Tree", menuName = "Skill Tree", order = 42)]
public class SkillTree : ScriptableObject
{
    [SerializeField]
    SkillNode rootSkill;

    int skillPoints = 0;
    int maxSkillPoints = 0;

    public SkillNode getRootSkill()
    {
        return rootSkill;
    }

    public void gainSkillPoints(int earned)
    {
        skillPoints += earned;
        maxSkillPoints += earned;
    }

    public void resetSkillPoints()
    {
        skillPoints = maxSkillPoints;
        rootSkill.disableSkill();
        rootSkill.enableSkill();
    }

    public List<Ability> getUnlockedAbilities()
    {
        return rootSkill.getUnlockedAbilities();
    }

    public bool unlockAbility(SkillNode skill)
    {
        if (skill.isUnlocked() && skill.getNodeCost() <= skillPoints)
        {
            skill.enableSkill();
            return true;
        }
        return false;
    }

    public int getAvailableSkillPoints()
    {
        return skillPoints;
    }

    public int getMaxSkillPoints()
    {
        return maxSkillPoints;
    }
}

[System.Serializable]
public class SkillNode
{
    [SerializeField]
    bool unlocked;

    [SerializeField]
    List<SkillNode> childSkills;

    [SerializeField]
    Ability ability;

    [SerializeField]
    int nodeCost;

    public bool isUnlocked()
    {
        return unlocked;
    }

    public List<SkillNode> getChildSkills()
    {
        return childSkills;
    }

    public Ability getAbility()
    {
        return ability;
    }

    public int getNodeCost()
    {
        return nodeCost;
    }

    public void disableSkill()
    {
        if (unlocked)
        {
            unlocked = false;
            foreach (SkillNode node in childSkills)
            {
                node.disableSkill();
            }
        }
    }

    public void enableSkill()
    {
        unlocked = true;
    }

    public List<Ability> getUnlockedAbilities()
    {
        List<Ability> abilities = new List<Ability>();
        if (unlocked)
        {
            abilities.Add(ability);
            foreach (SkillNode node in childSkills)
            {
                abilities.AddRange(node.getUnlockedAbilities());
            }
        }
        return abilities;
    }
}