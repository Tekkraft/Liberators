using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Object", order = 48)]
public class Weapon : ScriptableObject
{
    [SerializeField]
    string weaponName;

    [SerializeField]
    Sprite weaponIcon;

    [SerializeField]
    int weaponStrength;

    [SerializeField]
    int weaponHit;

    [SerializeField]
    int weaponCrit;

    [SerializeField]
    int weaponRange;

    [SerializeField]
    DamageElement weaponElement;

    [SerializeField]
    List<Ability> abilities;

    [SerializeField]
    Status weaponStatus;

    public string GetName()
    {
        return weaponName;
    }

    public Sprite GetSprite()
    {
        return weaponIcon;
    }

    public int[] GetWeaponStats()
    {
        return new int[] { weaponStrength, weaponHit, weaponCrit, weaponRange };
    }

    public DamageElement GetWeaponElement()
    {
        return weaponElement;
    }

    public List<Ability> GetAbilities()
    {
        return abilities;
    }

    public Status GetWeaponStatus()
    {
        return weaponStatus;
    }
}
