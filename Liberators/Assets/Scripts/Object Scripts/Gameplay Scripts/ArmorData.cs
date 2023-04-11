using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class ArmorData
{
    public string armorBaseId;

    public Armor LoadArmorData()
    {
        return Resources.Load<Armor>("Armors/" + armorBaseId);
    }

    //JSON Code
    public static ArmorData FromJSON(string jsonString)
    {
        return JsonUtility.FromJson<ArmorData>(jsonString);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }
}