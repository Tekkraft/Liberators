using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Combat Ability", menuName = "Combat Ability", order = 44)]
public class CombatAbility : Ability
{
    [SerializeField]
    bool isMelee;

    [SerializeField]
    targetType abilityTargetType;

    [SerializeField]
    int abilityRangeMax;

    [SerializeField]
    int abilityRangeMin;

    [SerializeField]
    bool fixedAbilityRange;

    //AOE Range
    [SerializeField]
    int abilityRadiusMax;

    [SerializeField]
    int abilityRadiusMin;

    //True Damage = Flat Damage
    [SerializeField]
    damageType abilityDamageSource;

    [SerializeField]
    damageType abilityDamageType;

    [SerializeField]
    bool damagingAbility = true;

    [SerializeField]
    int abilityDamage;

    [SerializeField]
    int abilityDamageBonus;

    [SerializeField]
    int abilityHitBonus;

    [SerializeField]
    bool trueHit;

    [SerializeField]
    int numberOfAttacks = 1;

    [SerializeField]
    bool requiresLOS = true;

    [SerializeField]
    bool requiresAOELOS = true;

    [SerializeField]
    bool hasOwnElement;

    [SerializeField]
    element abilityElement;

    [SerializeField]
    Status inflictStatus;

    //Special rules = Damage Ramp, etc.
    [SerializeField]
    List<string> specialRules;

    public bool getMelee()
    {
        return isMelee;
    }

    public targetType getTargetType()
    {
        return abilityTargetType;
    }

    public int[] getAbilityRanges()
    {
        return new int[] { abilityRangeMax, abilityRangeMin };
    }

    public bool getFixedAbilityRange()
    {
        return fixedAbilityRange;
    }

    public int[] getAbilityRadii()
    {
        return new int[] { abilityRadiusMax, abilityRadiusMin };
    }

    public damageType getAbilityDamageSource()
    {
        return abilityDamageSource;
    }

    public damageType getAbilityDamageType()
    {
        return abilityDamageType;
    }

    public bool getDamagingAbility()
    {
        return damagingAbility;
    }

    public int getAbilityDamage()
    {
        return abilityDamage;
    }

    public int getAbilityDamageBonus()
    {
        return abilityDamageBonus;
    }

    public int getAbilityHitBonus()
    {
        return abilityHitBonus;
    }

    public bool getTrueHit()
    {
        return trueHit;
    }

    public int getNumberOfAttacks()
    {
        return numberOfAttacks;
    }

    public bool getLOSRequirement()
    {
        return requiresLOS;
    }

    public bool getAOELOSRequirement()
    {
        return requiresAOELOS;
    }

    public bool hasElement()
    {
        return hasOwnElement;
    }

    public element getElement()
    {
        return abilityElement;
    }

    public Status getAbilityStatus()
    {
        return inflictStatus;
    }

    public List<string> getSpecialRules()
    {
        return specialRules;
    }
}
