using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability Object", order = 50)]
public class Ability : ScriptableObject
{
    [SerializeField]
    string abilityName;

    [SerializeField]
    Sprite abilitySprite;

    [SerializeField]
    int abilityAP;

    [SerializeField]
    actionType abilityType;

    [SerializeField]
    List<EquipHandClass> equipHandRequirement;

    [SerializeField]
    List<EquipBaseClass> equipBaseRequirement;

    [SerializeField]
    List<EquipDamageClass> equipDamageRequirement;

    public string getName()
    {
        return abilityName;
    }

    public Sprite getSprite()
    {
        return abilitySprite;
    }

    public int getAPCost()
    {
        return abilityAP;
    }

    public actionType getAbilityType()
    {
        return abilityType;
    }

    public List<EquipHandClass> GetHandRequirements()
    {
        return equipHandRequirement;
    }

    public List<EquipBaseClass> GetBaseRequirements()
    {
        return equipBaseRequirement;
    }

    public List<EquipDamageClass> GetDamageRequirements()
    {
        return equipDamageRequirement;
    }
}
