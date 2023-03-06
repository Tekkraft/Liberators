using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponInstance : ItemInstance
{
    [SerializeField]
    Weapon weaponBase;

    [SerializeField]
    WeaponCore weaponCore;

    [SerializeField]
    List<WeaponUpgrade> weaponUpgrades;

    [SerializeField]
    string instanceName;

    [SerializeField]
    Sprite instanceSprite;

    [SerializeField]
    int upgradeCapacity;

    public bool NullCheckBase()
    {
        return weaponBase == null;
    }

    public string GetInstanceName()
    {
        if (instanceName != null && instanceName != "")
        {
            return instanceName;
        }
        else
        {
            return weaponBase.GetName();
        }
    }

    public Sprite GetInstanceSprite()
    {
        if (instanceSprite != null)
        {
            return instanceSprite;
        }
        else
        {
            return weaponBase.GetSprite();
        }
    }

    public int GetMaxUpgradeCapacity()
    {
        return upgradeCapacity;
    }

    public int GetCurrentUpgradeCapacity()
    {
        int totalCost = 0;
        foreach (WeaponUpgrade upgrade in weaponUpgrades)
        {
            totalCost += upgrade.GetCapacityCost();
        }
        return upgradeCapacity - totalCost;
    }

    public EquipHandClass GetInstanceHandClass()
    {
        return weaponBase.GetHandClass();
    }

    public EquipBaseClass GetInstanceBaseClass()
    {
        return weaponBase.GetBaseClass();
    }

    public EquipDamageClass GetInstanceDamageClass()
    {
        return weaponBase.GetDamageClass();
    }

    public EquipSizeClass GetInstanceSizeClass()
    {
        return weaponBase.GetSizeClass();
    }

    public int[] GetInstanceWeaponStats()
    {
        return weaponBase.GetWeaponStats();
    }

    public DamageElement GetInstanceWeaponElement()
    {
        return weaponBase.GetWeaponElement();
    }

    public List<Ability> GetInstanceAbilities()
    {
        return weaponBase.GetAbilities();
    }

    public Status GetInstanceWeaponStatus()
    {
        return weaponBase.GetWeaponStatus();
    }
}

public class WeaponCore: ItemInstance
{
    [SerializeField]
    string instanceName;

    [SerializeField]
    Sprite instanceSprite;

    public string GetInstanceName()
    {
        return instanceName;
    }

    public Sprite GetInstanceSprite()
    {
        return instanceSprite;
    }
}

public class WeaponUpgrade: ItemInstance
{
    [SerializeField]
    string instanceName;

    [SerializeField]
    Sprite instanceSprite;

    [SerializeField]
    int capacityCost;

    public string GetInstanceName()
    {
        return instanceName;
    }

    public Sprite GetInstanceSprite()
    {
        return instanceSprite;
    }

    public int GetCapacityCost()
    {
        return capacityCost;
    }
}