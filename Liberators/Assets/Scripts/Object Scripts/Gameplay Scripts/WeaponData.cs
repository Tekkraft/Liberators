using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class WeaponData
{
    public string weaponBaseId;
    public string weaponCoreId;
    public List<string> weaponUpgradeId;
    public int upgradeCapacity;

    public Weapon LoadWeaponData()
    {
        return Resources.Load<Weapon>("Weapons/" + weaponBaseId);
    }

    //JSON Code
    public static WeaponData FromJSON(string jsonString)
    {
        return JsonUtility.FromJson<WeaponData>(jsonString);
    }

    public string ToJSON()
    {
        return JsonUtility.ToJson(this);
    }
}
