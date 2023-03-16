using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCalculator
{
    Grid mainGrid;
    MapController mapController;
    BattleController battleController;
    Vector2Int calculatorPosition;
    Vector2 targetDirection;
    BattleTeam targetTeam;

    CombatAbility linkedAbility;

    public AbilityCalculator(BattleTeam targetTeam, CombatAbility ability, Vector2Int calcPos, Vector2 targetDirection)
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        battleController = mainGrid.GetComponentsInChildren<BattleController>()[0];
        linkedAbility = ability;
        calculatorPosition = calcPos;
        this.targetDirection = targetDirection;
        this.targetTeam = targetTeam;
    }

    public List<GameObject> getAffectedUnits(TargetInstructionInstance targetInstruction, UnitController attackingUnit)
    {
        if (targetInstruction.getTargetType() != TargetType.BEAM)
        {
            return checkAOERange(targetInstruction, attackingUnit);
        }
        else
        {
            return checkBeamRange(attackingUnit);
        }
    }

    List<GameObject> checkAOERange(TargetInstructionInstance targetInstruction, UnitController attackingUnit)
    {
        int rangeMax = targetInstruction.getMaxRange();
        if (!targetInstruction.getMaxRangeFixed())
        {
            rangeMax += attackingUnit.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
        }
        int rangeMin = targetInstruction.getMinRange();
        if (!targetInstruction.getMinRangeFixed())
        {
            rangeMin += attackingUnit.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetInstruction.getLOSRequired(), mapController, battleController, battleController.GetTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsOfTeam(calculatorPosition, targetTeam, false);
        return hitUnits;
    }

    List<GameObject> checkBeamRange(UnitController attackingUnit)
    {
        AbilityData abilityData = linkedAbility.getAbilityData();
        TargetInstructionInstance targetInstruction = abilityData.getTargetInstruction();
        int rangeMax = targetInstruction.getMaxRange();
        if (!targetInstruction.getMaxRangeFixed())
        {
            rangeMax += attackingUnit.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
        }
        int rangeMin = targetInstruction.getMinRange();
        if (!targetInstruction.getMinRangeFixed())
        {
            rangeMin += attackingUnit.GetEquippedWeapons().Item1.GetInstanceWeaponStats()[3];
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetInstruction.getLOSRequired(), mapController, battleController, battleController.GetTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsOfTeam(calculatorPosition, targetTeam, true);
        return hitUnits;
    }
}
