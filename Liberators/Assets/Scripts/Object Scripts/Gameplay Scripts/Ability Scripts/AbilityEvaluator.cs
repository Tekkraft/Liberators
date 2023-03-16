using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;

public class AbilityEvaluator
{
    public static AbilityScript Deserialize<AbilityScript>(string filePath)
    {
        try
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AbilityScript));
            StreamReader streamReader = new StreamReader(Path.Combine(Application.dataPath, "Ability XML Files", filePath));
            AbilityScript result = (AbilityScript)xmlSerializer.Deserialize(streamReader.BaseStream);
            streamReader.Close();
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError("Exception importing xml file: " + e);
            return default;
        }
    }

    public static void LogScript(AbilityScript script)
    {
        RangeElement range = (script.targeting[0] as UnitTargeting).range;
        if (range.melee)
        {
            Debug.Log("Melee Range, LOS Required: " + range.sightRequired);
        }
        else
        {
            Debug.Log(range.min + "-" + range.max + ", Flexible Max Range: " + range.extendMaxRange + ", LOS Required: " + range.sightRequired);
        }
        TeamElement team = (script.targeting[0] as UnitTargeting).team;
        Debug.Log(team.filter);
        CountElement count = (script.targeting[0] as UnitTargeting).count;
        if (count != null)
        {
            Debug.Log(count.targets + " targets, duplicates:" + count.duplicate + ", manual: " + count.selectManual);
        }
        DamageEffect damage = script.effects[0] as DamageEffect;
        Debug.Log(damage.value + " points of " + damage.type + " " + damage.element + " damage based on " + damage.source + " stats");
    }
}
