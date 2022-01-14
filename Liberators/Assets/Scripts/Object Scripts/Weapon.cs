using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Object", order = 51)]
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
    int weaponRange;
}
