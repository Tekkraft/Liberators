using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill Tree", menuName = "Skill Tree", order = 42)]
public class SkillTree : ScriptableObject
{
    [SerializeField]
    SkillNode rootSkill;

    public SkillNode getRootSkill()
    {
        return rootSkill;
    }
}

[System.Serializable]
public class SkillNode
{
    [SerializeField]
    List<SkillNode> childSkills;

    [SerializeField]
    Ability ability;

    [SerializeField]
    int nodeCost;

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