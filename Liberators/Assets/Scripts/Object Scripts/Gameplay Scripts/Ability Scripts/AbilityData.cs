using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityData
{
    [SerializeField]
    TargetInstruction targetInstruction;

    [SerializeField]
    List<EffectInstruction> effectInstructions;

    public TargetInstruction getTargetInstruction()
    {
        return targetInstruction;
    }

    public List<EffectInstruction> getEffectInstructions()
    {
        return effectInstructions;
    }
}

[System.Serializable]
public class TargetInstruction
{
    [SerializeField]
    targetType targetType = targetType.NONE;

    [SerializeField]
    targetCondition targetCondition;

    [SerializeField]
    int targetConditionCount = 1;

    [SerializeField]
    List<TargetFilter> conditionFilters;

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

    public targetType getTargetType()
    {
        return targetType;
    }

    public targetCondition getTargetCondition()
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

    public List<TargetFilter> getConditionFilters()
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
public class EffectInstruction
{
    //SELF in this step refers to original target
    [SerializeField]
    bool independentHit;

    [SerializeField]
    TargetInstruction effectTarget;

    [SerializeField]
    effectType effectType;

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
    element effectElement;

    [SerializeField]
    Status effectStatus;

    [SerializeField]
    List<string> effectSpecialRules;

    public bool getIndependentHit()
    {
        return independentHit;
    }

    public TargetInstruction getEffectTarget()
    {
        return effectTarget;
    }

    public effectType getEffectType()
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

    public element getEffectElement()
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
public class TargetFilter
{
    [SerializeField]
    targetFilter targetFilter;

    [SerializeField]
    float filterStat = 0;

    public targetFilter getTargetFilter()
    {
        return targetFilter;
    }

    public float getFilterStat()
    {
        return filterStat;
    }
}

