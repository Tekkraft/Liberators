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

    public void setSkillPoints(int points)
    {
        skillPoints = points;
        maxSkillPoints = points;
        rootSkill.disableSkill();
        rootSkill.enableSkill();
    }

    public List<Ability> getUnlockedAbilities()
    {
        return rootSkill.getUnlockedAbilities();
    }

    public bool learnAbility(SkillNode skill)
    {
        if (skill.isUnlocked() && skill.getNodeCost() <= skillPoints)
        {
            skill.learnSkill();
            skillPoints -= skill.getNodeCost();
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
    bool obtained;

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

    public bool isObtained()
    {
        return obtained;
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
        obtained = false;
        unlocked = false;
        foreach (SkillNode node in childSkills)
        {
            node.disableSkill();
        }
    }

    public void enableSkill()
    {
        unlocked = true;
    }

    public void learnSkill()
    {
        obtained = true;
        unlocked = false;
        foreach (SkillNode node in childSkills)
        {
            node.enableSkill();
        }
    }

    public List<Ability> getUnlockedAbilities()
    {
        List<Ability> abilities = new List<Ability>();
        if (obtained)
        {
            abilities.Add(ability);
            foreach (SkillNode node in childSkills)
            {
                abilities.AddRange(node.getUnlockedAbilities());
            }
        }
        return abilities;
    }

    //Returns the total width above this node
    public int widthAboveNode()
    {
        int width = 0;
        foreach (SkillNode child in childSkills)
        {
            int childWidth = child.widthAboveNode();
            if (childWidth == 0)
            {
                childWidth = 1;
            }
            width += childWidth;
        }
        return width;
    }

    //Returns the total width of this node's children
    public List<int> widthOfChildren()
    {
        List<int> widths = new List<int>();
        foreach (SkillNode child in childSkills)
        {
            int childWidth = child.widthAboveNode();
            if (childWidth == 0)
            {
                childWidth = 1;
            }
            widths.Add(childWidth);
        }
        return widths;
    }
}