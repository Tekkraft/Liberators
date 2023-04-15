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

    public void Initialize(string source, string mainWeaponId, string secondaryWeaponId, string armorId)
    {
        this.source = source;
        Unit unit = Resources.Load<Unit>("Units/" + source);
        //TODO: TEMP CODE, REMOVE
        if (unit == null)
        {
            Debug.LogError("No unit template for unit: " + source);
            return;
        }
        unitName = unit.getUnitName();
        className = unit.getClassName();
        maxHP = unit.getStats()[0];
        currentHP = unit.getStats()[0];
        mov = unit.getStats()[1];
        str = unit.getStats()[2];
        pot = unit.getStats()[3];
        acu = unit.getStats()[4];
        fin = unit.getStats()[5];
        rea = unit.getStats()[6];
        if (mainWeaponId == "")
        {
            mainWeapon = null;
        }
        else
        {
            mainWeapon = new WeaponData(mainWeaponId);
        }
        if (secondaryWeaponId == "")
        {
            secondaryWeapon = null;
        }
        else
        {
            secondaryWeapon = new WeaponData(secondaryWeaponId);
        }
        if (armorId == "")
        {
            armor = null;
        }
        else
        {
            armor = new ArmorData(armorId);
        }
    }

    //Stat Functions
    public void SetCurrentHP(int newValue)
    {
        currentHP = newValue;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    public void ChangeCurrentHP(int changeAmount)
    {
        currentHP += changeAmount;
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
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