using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEntryHandler
{
    public static List<UnitEntryData> deployedUnits = new List<UnitEntryData>();

    public static void reset()
    {
        deployedUnits = new List<UnitEntryData>();
    }
}

[System.Serializable]
public class UnitEntryData
{
    [SerializeField]
    public Unit unit;
    [SerializeField]
    public Weapon weapon;
    [SerializeField]
    public Armor armor;

    public UnitEntryData(Unit unit, Weapon weapon, Armor armor)
    {
        this.unit = unit;
        this.weapon = weapon;
        this.armor = armor;
    }

    public Unit getUnit()
    {
        return unit;
    }

    public Weapon getWeapon()
    {
        return weapon;
    }

    public Armor getArmor()
    {
        return armor;
    }

    public void setUnit(Unit unit)
    {
        this.unit = unit;
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