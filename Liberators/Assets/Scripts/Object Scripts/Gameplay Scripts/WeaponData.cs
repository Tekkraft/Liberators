using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string weaponBaseId;
    public string weaponCoreId;
    public List<string> weaponUpgradeId;
    public int upgradeCapacity;

    public Weapon LoadWeaponData()
    {
        return Resources.Load(weaponBaseId) as Weapon;
    }
}
