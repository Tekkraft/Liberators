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
    public WeaponInstance mainHandWeapon;

    [SerializeField]
    public WeaponInstance offHandWeapon;

    [SerializeField]
    public ArmorInstance armor;

    public UnitEntryData(Unit unit, WeaponInstance mainHandWeapon, WeaponInstance offHandWeapon, ArmorInstance armor)
    {
        this.unit = unit;
        unitInstance = new UnitInstance(unit);
        this.mainHandWeapon = mainHandWeapon;
        this.offHandWeapon = offHandWeapon;
        this.armor = armor;
    }

    public void reconstruct()
    {
        if (!unit)
        {
            return;
        }
        unitInstance = new UnitInstance(unit);
    }

    public void setUnitCurrentHP(int currentHP)
    {
        unitInstance.setCurrentHP(currentHP);
    }

    public void doPostBattleHealing()
    {
        unitInstance.setCurrentHP(unitInstance.getCurrentHP() + Mathf.CeilToInt(unitInstance.getStats()[0] * 0.1f));
    }

    public UnitInstance getUnit()
    {
        return unitInstance;
    }

    public (WeaponInstance, WeaponInstance) getWeapons()
    {
        return (mainHandWeapon, offHandWeapon);
    }

    public ArmorInstance getArmor()
    {
        return armor;
    }

    public void setUnit(UnitInstance unitInstance)
    {
        this.unitInstance = unitInstance;
    }

    public void setWeapon(WeaponInstance weapon, bool mainSlot)
    {
        if (mainSlot)
        {
            mainHandWeapon = weapon;
        }
        else
        {
            offHandWeapon = weapon;
        }
    }

    public void setArmor(ArmorInstance armor)
    {
        this.armor = armor;
    }
}
