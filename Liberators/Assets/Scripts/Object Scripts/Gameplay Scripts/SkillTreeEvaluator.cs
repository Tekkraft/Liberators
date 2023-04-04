using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;

public class SkillTreeEvaluator
{
    public static SkillTreeScript Deserialize<SkillTreeScript>(string filePath)
    {
        try
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AbilityScript));
            StreamReader streamReader = new StreamReader(Path.Combine(Application.dataPath, "XML", "Skill Tree XML Files", filePath));
            SkillTreeScript result = (SkillTreeScript)xmlSerializer.Deserialize(streamReader.BaseStream);
            streamReader.Close();
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception importing xml file: " + e);
            return default;
        }
    }

    public static SkillTreeData CreateTreeData(string filePath)
    {
        SkillTreeScript script = Deserialize<SkillTreeScript>(filePath);
        SkillTreeData data = new SkillTreeData();
        Dictionary<SkillNodeScript, SkillNodeData> linker = new Dictionary<SkillNodeScript, SkillNodeData>();
        Dictionary<string, SkillNodeScript> idLinker = new Dictionary<string, SkillNodeScript>();
        foreach (SkillNodeScript node in script.nodes)
        {
            SkillNodeData nodeData = new SkillNodeData();
            linker.Add(node, nodeData);
            idLinker.Add(node.id, node);
            nodeData.id = node.id;
            nodeData.cost = node.cost;
            nodeData.unlocked = node.starting;
            data.nodes.Add(nodeData);
        }
        foreach (SkillNodeScript node in script.nodes)
        {
            SkillNodeData nodeData = linker[node];
            foreach (NodeReference reference in node.nodes)
            {
                if (idLinker[reference.id] == null)
                {
                    continue;
                }
                nodeData.requirements.Add(linker[idLinker[reference.id]]);
            }
            foreach (SkillReference reference in node.skills)
            {
                nodeData.unlocks.Add(reference.name);
            }
        }
        return data;
    }
}
