using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class UnitData
{
    public string source;

    public string unitName;
    public string className;

    public int maxHP;
    public int currentHP;
    public int mov;
    public int str;
    public int pot;
    public int acu;
    public int fin;
    public int rea;

    //Skill Tree Data
    public int maxSkillPoints;
    public int availableSkillPoints;
    public SkillTreeData skillTree;

    //Equipment Data
    public WeaponData mainWeapon;
    public WeaponData secondaryWeapon;
    public ArmorData armor;

    //Battle Animation
    //TODO: Implement more permanent animation system
    public Sprite GetBattleSprite(string tag)
    {
        Unit unit = Resources.Load<Unit>("Units/" + source);
        return unit.getBattleSprite(tag);
    }

    //JSON Code
    public static UnitData FromJson(string jsonString)
    {
        return JsonUtility.FromJson<UnitData>(jsonString);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}