using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatOperation
{
    public enum operationType { DAMAGE, STATUS, HEAL, REDIRECT }

    GameObject origin;
    GameObject target;
    int priority;
    operationType combatOpType;
    int amount;

    CombatOperation(GameObject origin, GameObject target, int priority, operationType combatOpType, int amount)
    {
        this.origin = origin;
        this.target = target;
        this.priority = priority;
        this.combatOpType = combatOpType;
        this.amount = amount;
    }

    public GameObject getOrigin()
    {
        return origin;
    }

    public GameObject getTarget()
    {
        return target;
    }

    public int getPriority()
    {
        return priority;
    }

    public operationType getOperationType()
    {
        return combatOpType;
    }

    public int getAmount()
    {
        return amount;
    }
}
