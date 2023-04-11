using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SkillTreeData
{
    public string source;
    public List<SkillNodeData> nodes;

    public void Initialize()
    {
        foreach (SkillNodeData node in nodes)
        {
            node.Initialize(this);
        }
    }

    public List<List<SkillNodeData>> NodesByDepth()
    {
        List<List<SkillNodeData>> nodeList = new List<List<SkillNodeData>>();
        int depth = 0;
        while (depth < nodes.Count)
        {
            List<SkillNodeData> subList = NodesAtDepth(depth);
            if (subList.Count <= 0)
            {
                break;
            }
            nodeList.Add(subList);
            depth++;
        }
        return nodeList;
    }

    public List<SkillNodeData> NodesAtDepth(int depth)
    {
        List<SkillNodeData> nodeList = new List<SkillNodeData>();
        foreach (SkillNodeData node in nodes)
        {
            if (node.depth == depth)
            {
                nodeList.Add(node);
            }
        }
        return nodeList;
    }

    public SkillNodeData SearchById(string id)
    {
        foreach (SkillNodeData node in nodes)
        {
            if (node.id == id)
            {
                return node;
            }
        }
        return null;
    }

    public void CheckAllUnlocked()
    {
        foreach (SkillNodeData data in nodes)
        {
            data.CheckUnlocked(this);
        }
    }

    public void LoadAllLearnedAbilities()
    {
        foreach (SkillNodeData data in nodes)
        {
            data.LoadLearnedAbilities();
        }
    }

    public List<Ability> GetAllLearnedAbilities()
    {
        List<Ability> abilities = new List<Ability>();
        foreach (SkillNodeData data in nodes)
        {
            if (data.learned)
            {
                abilities.AddRange(data.unlockedAbilties);
            }
        }
        return abilities;
    }

    public static SkillTreeData FromXML(string xmlPath)
    {
        return SkillTreeEvaluator.CreateTreeData(xmlPath);
    }

    public static SkillTreeData FromJSON(string jsonString)
    {
        return JsonUtility.FromJson<SkillTreeData>(jsonString);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class SkillNodeData
{
    public string id;
    public bool unlocked;
    public bool learned;
    public int cost;
    public List<string> requirements;
    public List<string> unlocks;
    public List<Ability> unlockedAbilties = new List<Ability>();
    public int depth = 0;

    public void Initialize(SkillTreeData parent)
    {
        if (requirements.Count <= 0)
        {
            depth = 0;
        }
        else
        {
            depth = 0;
            foreach (string id in requirements)
            {
                if (parent.SearchById(id).depth >= depth)
                {
                    depth = parent.SearchById(id).depth + 1;
                }
            }
        }
    }

    public void CheckUnlocked(SkillTreeData parent)
    {
        foreach (string id in requirements)
        {
            if (!parent.SearchById(id).learned)
            {
                unlocked = false;
                return;
            }
        }
        unlocked = true;
    }

    public void LoadLearnedAbilities()
    {
        foreach(string abilityName in unlocks)
        {
            unlockedAbilties.Add(Resources.Load<Ability>("Abilities/" + abilityName));
        }
    }
}