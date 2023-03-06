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
    StatusType statusType;

    [SerializeField]
    bool statusVisible;

    //Direct Stat Changes
    [SerializeField]
    bool hasStatChanges;

    //False here equals percentile
    [SerializeField]
    bool flatStatChange;

    [SerializeField]
    int maxHP;

    //[SerializeField]
    int mov = 0;

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
    DamageElement healthChangeElement;

    //Action Changes
    [SerializeField]
    int actionPointChange;

    [SerializeField]
    bool setAPMode;

    //Movement Changes
    [SerializeField]
    int movementChange;

    [SerializeField]
    bool setMoveMode;

    //Special Properties
    [SerializeField]
    List<string> specialProperties;

    public string getName()
    {
        return statusName;
    }

    public int getDuration()
    {
        return statusDuration;
    }

    public StatusType getStatusType()
    {
        return statusType;
    }

    public bool isVisible()
    {
        return statusVisible;
    }

    public bool statChanges()
    {
        return hasStatChanges;
    }

    public bool statChangeType()
    {
        return flatStatChange;
    }

    public int[] getStatChanges()
    {
        return new int[] { maxHP, mov, str, pot, acu, fin, rea };
    }

    public int[] getHealthOverTime()
    {
        return new int[] { damageOverTime, healingOverTime };
    }

    public DamageElement getHealthOverTimeElement()
    {
        return healthChangeElement;
    }

    public int getAPChange()
    {
        return actionPointChange;
    }

    public bool getAPMode()
    {
        return setAPMode;
    }

    public int getMoveChange()
    {
        return movementChange;
    }

    public bool getMoveMode()
    {
        return setMoveMode;
    }

    public List<string> getSpecialProperties()
    {
        return specialProperties;
    }
}