using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitEntryData
{
    [SerializeField]
    public Unit unit;
    UnitInstance unitInstance;

    [SerializeField]
    public Weapon weapon;

    [SerializeField]
    public Armor armor;

    SkillTreeInstance skillTree;

    public UnitEntryData(Unit unit, Weapon weapon, Armor armor)
    {
        this.unit = unit;
        unitInstance = new UnitInstance(unit);
        this.weapon = weapon;
        this.armor = armor;
        skillTree = new SkillTreeInstance(unit.getSkillTree());
    }

    public void reconstruct()
    {
        if (!unit)
        {
            return;
        }
        unitInstance = new UnitInstance(unit);
    }

    public UnitInstance getUnit()
    {
        return unitInstance;
    }

    public Weapon getWeapon()
    {
        return weapon;
    }

    public Armor getArmor()
    {
        return armor;
    }

    public void setUnit(UnitInstance unitInstance)
    {
        this.unitInstance = unitInstance;
    }

    public void setWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public void setArmor(Armor armor)
    {
        this.armor = armor;
    }
}
