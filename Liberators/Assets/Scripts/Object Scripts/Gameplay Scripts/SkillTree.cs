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

    //Get the max widths of all children of root
    public List<int> getChildWidths(SkillNode root)
    {
        List<int> widths = new List<int>();
        foreach(SkillNode node in root.getChildSkills())
        {
            widths.Add(getMaxWidth(node));
        }
        return widths;
    }

    //Return the highest width for the tree starting at root;
    public int getMaxWidth(SkillNode root)
    {
        List<int> widths = getTreeWidths(root);
        int maxWidth = -1;
        foreach (int width in widths)
        {
            if (width > maxWidth)
            {
                maxWidth = width;
            }
        }
        return maxWidth;
    }

    //Returns the total widths of the tree by level starting at root
    public List<int> getTreeWidths(SkillNode root)
    {
        List<int> widths = new List<int>();
        List<SkillNode> nodes;
        int index = 0;
        nodes = nodesAtLevel(index, root);
        while (nodes.Count > 0)
        {
            widths.Add(nodes.Count);
            index++;
            nodes.Clear();
            nodes = nodesAtLevel(index, root);
        }
        return widths;
    }

    //Returns the number of nodes at any given level
    List<SkillNode> nodesAtLevel(int level, SkillNode root)
    {
        List<SkillNode> nodes = new List<SkillNode>();
        int layerLevel = 0;
        nodes.Add(root);
        while (layerLevel < level)
        {
            List<SkillNode> tempNodes = new List<SkillNode>();
            tempNodes.AddRange(nodes);
            nodes.Clear();
            foreach (SkillNode node in tempNodes)
            {
                nodes.AddRange(node.getChildSkills());
            }
            layerLevel++;
        }
        return nodes;
    }
}