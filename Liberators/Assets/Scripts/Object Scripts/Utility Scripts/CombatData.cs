using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatData
{
    GameObject attacker, defender;
    EffectInstruction attackEffect;
    bool attackHit, attackCrit;
    int damageDealt, startingHP;
    bool defenderKilled;
    Status status;
    bool statusInflicted;
    combatDataType combatDataType;

    public CombatData(GameObject attacker, GameObject defender, EffectInstruction attackEffect, bool attackHit, bool attackCrit, int damageDealt, int startingHP, bool defenderKilled)
    {
        combatDataType = combatDataType.DAMAGE;
        this.attacker = attacker;
        this.defender = defender;
        this.attackEffect = attackEffect;
        this.attackHit = attackHit;
        this.attackCrit = attackCrit;
        this.damageDealt = damageDealt;
        this.startingHP = startingHP;
        this.defenderKilled = defenderKilled;
    }

    public CombatData(GameObject attacker, GameObject defender, Status status, bool statusInflicted)
    {
        combatDataType = combatDataType.STATUS;
        this.attacker = attacker;
        this.defender = defender;
        this.status = status;
        this.statusInflicted = statusInflicted;
    }

    public CombatData()
    {
        combatDataType = combatDataType.BREAK;
    }

    public GameObject getAttacker()
    {
        return attacker;
    }

    public GameObject getDefender()
    {
        return defender;
    }

    public EffectInstruction getEffectInstruction()
    {
        return attackEffect;
    }

    public bool getAttackHit()
    {
        return attackHit;
    }

    public bool getAttackCrit()
    {
        return attackCrit;
    }

    public int getDamageDealt()
    {
        return damageDealt;
    }

    public int getStartingHP()
    {
        return startingHP;
    }

    public int getEndingHP()
    {
        return startingHP - damageDealt;
    }

    public bool getDefenderKilled()
    {
        return defenderKilled;
    }

    public Status getStatus()
    {
        return status;
    }

    public bool getStatusInflicted()
    {
        return statusInflicted;
    }

    public combatDataType getCombatDataType()
    {
        return combatDataType;
    }
}
