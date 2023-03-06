using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityData
{
    [SerializeField]
    TargetInstructionInstance targetInstruction;

    [SerializeField]
    List<EffectInstructionInstance> effectInstructions;

    [SerializeField]
    int abilityRepeats = 1;

    public TargetInstructionInstance getTargetInstruction()
    {
        return targetInstruction;
    }

    public List<EffectInstructionInstance> getEffectInstructions()
    {
        return effectInstructions;
    }

    public int getAbilityRepeats()
    {
        return abilityRepeats;
    }
}

[System.Serializable]
public class TargetInstructionInstance
{
    [SerializeField]
    TargetType targetType = TargetType.NONE;

    [SerializeField]
    TargetCondition targetCondition;

    [SerializeField]
    int targetConditionCount = 1;

    [SerializeField]
    List<TargetFilterInstance> conditionFilters;

    [SerializeField]
    bool isMelee = false;

    [SerializeField]
    bool requiresLOS = true;

    [SerializeField]
    int minRange = 0;

    [SerializeField]
    bool minRangeFixed = true;

    [SerializeField]
    int maxRange = 0;

    [SerializeField]
    bool maxRangeFixed = false;

    [SerializeField]
    int hitBonus;

    [SerializeField]
    bool fixedHit;

    public TargetType getTargetType()
    {
        return targetType;
    }

    public TargetCondition getTargetCondition()
    {
        return targetCondition;
    }

    public int getTargetConditionCount()
    {
        return targetConditionCount;
    }

    public bool getIsMelee()
    {
        return isMelee;
    }

    public bool getLOSRequired()
    {
        return requiresLOS;
    }

    public int getMinRange()
    {
        return minRange;
    }

    public bool getMinRangeFixed()
    {
        return minRangeFixed;
    }

    public int getMaxRange()
    {
        return maxRange;
    }

    public bool getMaxRangeFixed()
    {
        return maxRangeFixed;
    }

    public List<TargetFilterInstance> getConditionFilters()
    {
        return conditionFilters;
    }

    public int getHitBonus()
    {
        return hitBonus;
    }

    public bool getFixedHit()
    {
        return fixedHit;
    }
}

[System.Serializable]
public class EffectInstructionInstance
{
    //SELF in this step refers to original target
    [SerializeField]
    bool independentHit;

    [SerializeField]
    TargetInstructionInstance effectTarget;

    [SerializeField]
    EffectType effectType;

    [SerializeField]
    damageType effectDamageType;

    [SerializeField]
    damageType effectDamageSource;

    [SerializeField]
    int effectIntensity;

    [SerializeField]
    int effectPercentBonus;

    [SerializeField]
    bool effectIndependentElement = false;

    [SerializeField]
    DamageElement effectElement;

    [SerializeField]
    Status effectStatus;

    [SerializeField]
    List<string> effectSpecialRules;

    public bool getIndependentHit()
    {
        return independentHit;
    }

    public TargetInstructionInstance getEffectTarget()
    {
        return effectTarget;
    }

    public EffectType getEffectType()
    {
        return effectType;
    }

    public damageType getEffectDamageType()
    {
        return effectDamageType;
    }

    public damageType getEffectDamageSource()
    {
        return effectDamageSource;
    }

    public int getEffectIntensity()
    {
        return effectIntensity;
    }

    public int getEffectPercentBonus()
    {
        return effectPercentBonus;
    }

    public bool getEffectIndependentElement()
    {
        return effectIndependentElement;
    }

    public DamageElement getEffectElement()
    {
        return effectElement;
    }

    public Status getEffectStatus()
    {
        return effectStatus;
    }

    public List<string> getSpecialRules()
    {
        return effectSpecialRules;
    }
}

[System.Serializable]
public class TargetFilterInstance
{
    [SerializeField]
    TargetFilter targetFilter;

    [SerializeField]
    float filterStat = 0;

    public TargetFilter getTargetFilter()
    {
        return targetFilter;
    }

    public float getFilterStat()
    {
        return filterStat;
    }
}

