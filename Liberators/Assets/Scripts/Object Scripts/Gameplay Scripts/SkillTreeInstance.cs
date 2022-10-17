using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeInstance
{
    SkillTree linkedTree;

    List<SkillNode> unlockedSkills = new List<SkillNode>();
    List<SkillNode> obtainedSkills = new List<SkillNode>();

    int skillPoints = 0;
    int maxSkillPoints = 0;

    public SkillTreeInstance(SkillTree tree)
    {
        linkedTree = tree;
        unlockedSkills.Add(linkedTree.getRootSkill());
    }

    public SkillNode getRootNode()
    {
        return linkedTree.getRootSkill();
    }

    public void gainSkillPoints(int earned)
    {
        skillPoints += earned;
        maxSkillPoints += earned;
    }

    public void resetSkillPoints()
    {
        skillPoints = maxSkillPoints;
        unlockedSkills.Clear();
        obtainedSkills.Clear();
        unlockedSkills.Add(linkedTree.getRootSkill());
    }

    public bool learnAbility(SkillNode skill)
    {
        if (unlockedSkills.Contains(skill) && skill.getNodeCost() <= skillPoints)
        {
            unlockedSkills.Remove(skill);
            obtainedSkills.Add(skill);
            unlockedSkills.AddRange(skill.getChildSkills());
            skillPoints -= skill.getNodeCost();
            return true;
        }
        return false;
    }

    public List<Ability> getUnlockedAbilities()
    {
        List<Ability> available = new List<Ability>();
        foreach (SkillNode node in unlockedSkills)
        {
            available.Add(node.getAbility());
        }
        return available;
    }

    public List<Ability> getObtainedAbilities()
    {
        List<Ability> available = new List<Ability>();
        foreach (SkillNode node in obtainedSkills)
        {
            available.Add(node.getAbility());
        }
        return available;
    }

    public List<SkillNode> getUnlockedSkills()
    {
        return unlockedSkills;
    }

    public List<SkillNode> getObtainedSkills()
    {
        return obtainedSkills;
    }

    public bool skillUnlocked(SkillNode skill)
    {
        return unlockedSkills.Contains(skill);
    }

    public bool skillObtained(SkillNode skill)
    {
        return obtainedSkills.Contains(skill);
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
