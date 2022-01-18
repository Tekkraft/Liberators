using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability Object", order = 50)]
public class Ability : ScriptableObject
{
    public enum damageType{ PHYSICAL, MAGIC, TRUE }

    [SerializeField]
    string abilityName;

    [SerializeField]
    int abilityAP;

    [SerializeField]
    MapController.actionType abilityType;

    [SerializeField]
    int abilityRange;

    [SerializeField]
    UnitController.MarkerAreas abilityRangeType;

    [SerializeField]
    int abilityRadius;

    [SerializeField]
    UnitController.MarkerAreas abilityRadiusType;

    [SerializeField]
    damageType abilityDamageType;

    [SerializeField]
    bool abilityDamageFixed;

    [SerializeField]
    int abilityDamage;
}
