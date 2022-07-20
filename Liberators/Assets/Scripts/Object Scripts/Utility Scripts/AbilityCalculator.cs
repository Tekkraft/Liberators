using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCalculator
{
    Grid mainGrid;
    MapController mapController;
    Vector2Int calculatorPosition;
    Vector2 targetDirection;
    List<int> targetTeams;

    CombatAbility linkedAbility;

    public AbilityCalculator(List<int> targetTeams, CombatAbility ability, Vector2Int calcPos, Vector2 targetDirection)
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        linkedAbility = ability;
        calculatorPosition = calcPos;
        this.targetDirection = targetDirection;
        this.targetTeams = targetTeams;
    }

    public List<GameObject> getAffectedUnits()
    {
        if (linkedAbility.getTargetType() != targetType.BEAM)
        {
            return checkAOERange();
        }
        else
        {
            return checkBeamRange();
        }
    }

    void modifyEffect()
    {

    }

    List<GameObject> checkAOERange()
    {
        int rangeMax = linkedAbility.getAbilityRadii()[0];
        int rangeMin = linkedAbility.getAbilityRadii()[1];
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, linkedAbility.getAOELOSRequirement(), mapController, mapController.getTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsOfTeam(calculatorPosition, targetTeams, false);
        return hitUnits;
    }

    List<GameObject> checkBeamRange()
    {
        int rangeMax = linkedAbility.getAbilityRadii()[0];
        int rangeMin = linkedAbility.getAbilityRadii()[1];
        Rangefinder rangefinder = new Rangefinder(rangeMax, rangeMin, linkedAbility.getLOSRequirement(), mapController, mapController.getTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsOfTeam(calculatorPosition, targetTeams, true);
        return hitUnits;
    }
}
