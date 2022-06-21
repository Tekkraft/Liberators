using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Object", order = 48)]
public class Weapon : ScriptableObject
{
    [SerializeField]
    string weaponName;

    [SerializeField]
    WeaponType weaponType;

    [SerializeField]
    int weaponStrength;

    [SerializeField]
    int weaponHit;

    [SerializeField]
    int weaponCrit;

    [SerializeField]
    int weaponRange;

    [SerializeField]
    element weaponElement;

    [SerializeField]
    List<Ability> abilities;

    [SerializeField]
    Status weaponStatus;

    public string getWeaponName()
    {
        return weaponName;
    }

    public WeaponType getWeaponType()
    {
        return weaponType;
    }

    public int[] getWeaponStats()
    {
        return new int[] { weaponStrength, weaponHit, weaponCrit, weaponRange };
    }

    public element getWeaponElement()
    {
        return weaponElement;
    }

    public List<Ability> getAbilities()
    {
        return abilities;
    }

    public Status getWeaponStatus()
    {
        return weaponStatus;
    }
}
