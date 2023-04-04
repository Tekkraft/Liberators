using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitData
{
    public int maxHP { get; set; }
    public int currentHP { get; set; }
    public int mov { get; set; }
    public int str { get; set; }
    public int pot { get; set; }
    public int acu { get; set; }
    public int fin { get; set; }
    public int rea { get; set; }

    public List<Ability> prfAbilities;

    //Skill Tree Data
    public SkillTreeData skillTree;

    //Equipment Data
    public WeaponData mainWeapon;
    public WeaponData secondaryWeapon;
    public ArmorData armor;

    //Battle Animation
}