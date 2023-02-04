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
    battleTeam targetTeam;

    CombatAbility linkedAbility;

    public AbilityCalculator(battleTeam targetTeam, CombatAbility ability, Vector2Int calcPos, Vector2 targetDirection)
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        battleController = mainGrid.GetComponentsInChildren<BattleController>()[0];
        linkedAbility = ability;
        calculatorPosition = calcPos;
        this.targetDirection = targetDirection;
        this.targetTeam = targetTeam;
    }

    public List<GameObject> getAffectedUnits(TargetInstruction targetInstruction, UnitController attackingUnit)
    {
        if (targetInstruction.getTargetType() != targetType.BEAM)
        {
            return checkAOERange(targetInstruction, attackingUnit);
        }
        else
        {
            return checkBeamRange(attackingUnit);
        }
    }

    void modifyEffect()
    {

    }

    List<GameObject> checkAOERange(TargetInstruction targetInstruction, UnitController attackingUnit)
    {
        int rangeMax = targetInstruction.getMaxRange();
        if (!targetInstruction.getMaxRangeFixed())
        {
            rangeMax += attackingUnit.getEquippedWeapon().getWeaponStats()[3];
        }
        int rangeMin = targetInstruction.getMinRange();
        if (!targetInstruction.getMinRangeFixed())
        {
            rangeMin += attackingUnit.getEquippedWeapon().getWeaponStats()[3];
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetInstruction.getLOSRequired(), mapController, battleController, battleController.getTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsOfTeam(calculatorPosition, targetTeam, false);
        return hitUnits;
    }

    List<GameObject> checkBeamRange(UnitController attackingUnit)
    {
        AbilityData abilityData = linkedAbility.getAbilityData();
        TargetInstruction targetInstruction = abilityData.getTargetInstruction();
        int rangeMax = targetInstruction.getMaxRange();
        if (!targetInstruction.getMaxRangeFixed())
        {
            rangeMax += attackingUnit.getEquippedWeapon().getWeaponStats()[3];
        }
        int rangeMin = targetInstruction.getMinRange();
        if (!targetInstruction.getMinRangeFixed())
        {
            rangeMin += attackingUnit.getEquippedWeapon().getWeaponStats()[3];
        }
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, targetInstruction.getLOSRequired(), mapController, battleController, battleController.getTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsOfTeam(calculatorPosition, targetTeam, true);
        return hitUnits;
    }
}
