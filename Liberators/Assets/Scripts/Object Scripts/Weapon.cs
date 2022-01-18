using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Object", order = 48)]
public class Weapon : ScriptableObject
{
    public enum WeaponType { MELEE, GUN, FOCUS, SPECIAL };

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
    int ammoCapacity;

    [SerializeField]
    List<Ability> abilities;
}
