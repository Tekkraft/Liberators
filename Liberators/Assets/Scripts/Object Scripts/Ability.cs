using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability Object", order = 50)]
public class Ability : ScriptableObject
{
    public enum damageType { PHYSICAL, MAGIC, TRUE }

    [SerializeField]
    string abilityName;

    [SerializeField]
    int abilityAP;

    [SerializeField]
    MapController.actionType abilityType;

    //Attack and Support - Range of targeting. Add to base range if weapon skill.
    //Move - Flat move bonus
    [SerializeField]
    int abilityRange;

    [SerializeField]
    UnitController.MarkerAreas abilityRangeType;

    //Attack and Support - Range of effect
    //Move - Bonus move multiplier out of 100
    [SerializeField]
    int abilityRadius;

    //Unusued for Move
    [SerializeField]
    UnitController.MarkerAreas abilityRadiusType;

    //True Damage = Flat Damage
    [SerializeField]
    damageType abilityDamageSource;

    [SerializeField]
    damageType abilityDamageType;

    [SerializeField]
    int abilityDamage;

    public string getName()
    {
        return abilityName;
    }

    public int getAPCost()
    {
        return abilityAP;
    }

    public MapController.actionType getAbilityType()
    {
        return abilityType;
    }

    public int getAbilityRange()
    {
        return abilityRange;
    }

    public int getAbilityRadius()
    {
        return abilityRadius;
    }

    public UnitController.MarkerAreas getAbilityRangeType()
    {
        return abilityRangeType;
    }

    public UnitController.MarkerAreas getAbilityRadiusType()
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
}
