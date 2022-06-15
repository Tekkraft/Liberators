using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability Object", order = 50)]
public class Ability : ScriptableObject
{

    [SerializeField]
    string abilityName;

    [SerializeField]
    int abilityAP;

    [SerializeField]
    actionType abilityType;

    [SerializeField]
    bool isMelee;

    [SerializeField]
    targetType abilityTargetType;

    //Attack and Support - Range of targeting. Add to base range if weapon skill.
    //Move - Flat move bonus
    [SerializeField]
    int abilityRangeMax;

    //Unused for Move
    [SerializeField]
    int abilityRangeMin;

    [SerializeField]
    markerAreas abilityRangeType;

    //Attack and Support - Range of effect
    //Move - Bonus move multiplier out of 100
    [SerializeField]
    int abilityRadiusMax;

    //Unusued for Move
    [SerializeField]
    int abilityRadiusMin;

    //Unusued for Move
    [SerializeField]
    markerAreas abilityRadiusType;

    //True Damage = Flat Damage
    [SerializeField]
    damageType abilityDamageSource;

    [SerializeField]
    damageType abilityDamageType;

    [SerializeField]
    int abilityDamage;

    [SerializeField]
    int abilityHitBonus;

    [SerializeField]
    bool trueHit;

    [SerializeField]
    bool requiresLOS;

    [SerializeField]
    bool requiresAOELOS;

    //Special rules = Line, Damage Ramp, etc.
    [SerializeField]
    List<string> specialRules;

    public string getName()
    {
        return abilityName;
    }

    public int getAPCost()
    {
        return abilityAP;
    }

    public actionType getAbilityType()
    {
        return abilityType;
    }

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

    public int[] getAbilityRadii()
    {
        return new int[] { abilityRadiusMax, abilityRadiusMin };
    }

    public markerAreas getAbilityRangeType()
    {
        return abilityRangeType;
    }

    public markerAreas getAbilityRadiusType()
    {
        return abilityRadiusType;
    }

    public damageType getAbilityDamageSource()
    {
        return abilityDamageSource;
    }

    public damageType getAbilityDamageType()
    {
        return abilityDamageType;
    }

    public int getAbilityDamage()
    {
        return abilityDamage;
    }

    public int getAbilityHitBonus()
    {
        return abilityHitBonus;
    }

    public bool getTrueHit()
    {
        return trueHit;
    }

    public bool getLOSRequirement()
    {
        return requiresLOS;
    }

    public bool getAOELOSRequirement()
    {
        return requiresAOELOS;
    }

    public List<string> getSpecialRules()
    {
        return specialRules;
    }
}
