using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status", menuName = "Status Object", order = 46)]
public class Status : ScriptableObject
{
    [SerializeField]
    string statusName;

    [SerializeField]
    int statusDuration;

    [SerializeField]
    statusType statusType;

    [SerializeField]
    bool statusVisible;

    //Direct Stat Changes
    //False here equals percentile
    [SerializeField]
    bool flatStatChange;

    [SerializeField]
    int maxHP;

    [SerializeField]
    int mov;

    [SerializeField]
    int str;

    [SerializeField]
    int pot;

    [SerializeField]
    int acu;

    [SerializeField]
    int fin;

    [SerializeField]
    int rea;

    //Damage/Heal Over Time
    [SerializeField]
    int damageOverTime;

    [SerializeField]
    int healingOverTime;

    [SerializeField]
    element healthChangeElement;

    //Action Changes
    [SerializeField]
    int actionPointChange;

    [SerializeField]
    int actionPointSet;

    [SerializeField]
    bool setAPActive;

    //Movement Changes
    [SerializeField]
    int movementChange;

    [SerializeField]
    int movementSet;

    [SerializeField]
    bool setMoveActive;

    //Special Properties
    [SerializeField]
    List<string> specialProperties;
}
