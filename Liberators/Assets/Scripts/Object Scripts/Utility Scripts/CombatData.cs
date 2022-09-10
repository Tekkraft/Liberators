using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatData
{
    GameObject attacker, defender;
    EffectInstruction attackEffect;
    bool attackHit, attackCrit;
    int damageDealt;
    bool defenderKilled;

    public CombatData(GameObject attacker, GameObject defender, EffectInstruction attackEffect, bool attackHit, bool attackCrit, int damageDealt, bool defenderKilled)
    {
        this.attacker = attacker;
        this.defender = defender;
        this.attackEffect = attackEffect;
        this.attackHit = attackHit;
        this.attackCrit = attackCrit;
        this.damageDealt = damageDealt;
        this.defenderKilled = defenderKilled;
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

    public bool getDefenderKilled()
    {
        return defenderKilled;
    }
}
