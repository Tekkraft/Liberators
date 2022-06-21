using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCalculator
{
    Grid mainGrid;
    MapController mapController;
    Canvas uiCanvas;
    Vector2Int calculatorPosition;
    Vector2 targetDirection;

    GameObject linkedUnit;
    UnitController linkedController;
    Ability linkedAbility;

    List<GameObject> hitTargets;

    public AbilityCalculator(GameObject unit, Ability ability, Vector2Int calcPos, Vector2 targetDirection)
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        linkedUnit = unit;
        linkedController = linkedUnit.GetComponent<UnitController>();
        linkedAbility = ability;
        calculatorPosition = calcPos;
        this.targetDirection = targetDirection;
    }

    public List<GameObject> getAffectedUnits(bool checkAOE)
    {
        if (checkAOE)
        {
            return checkAOERange();
        }
        else if (linkedAbility.getTargetType() != targetType.BEAM)
        {
            return checkAreaRange();
        }
        else
        {
            return checkBeamRange();
        }
    }

    void modifyEffect()
    {

    }

    List<GameObject> checkAreaRange()
    {
        int rangeMax = linkedAbility.getAbilityRadii()[0];
        int rangeMin = linkedAbility.getAbilityRadii()[1];
        Rangefinder rangefinder = new Rangefinder(linkedUnit, rangeMax, rangeMin, linkedAbility.getLOSRequirement(), mapController, mapController.getTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsNotOfTeam(linkedUnit.GetComponent<UnitController>().getTeam(), false);
        return hitUnits;
    }

    List<GameObject> checkAOERange()
    {
        int rangeMax = linkedAbility.getAbilityRadii()[0];
        int rangeMin = linkedAbility.getAbilityRadii()[1];
        Rangefinder rangefinder = new Rangefinder(linkedUnit, rangeMax, rangeMin, linkedAbility.getLOSRequirement(), mapController, mapController.getTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsOfTeam(linkedUnit.GetComponent<UnitController>().getTeam(), false);
        return hitUnits;
    }

    List<GameObject> checkBeamRange()
    {
        int rangeMax = linkedAbility.getAbilityRadii()[0];
        int rangeMin = linkedAbility.getAbilityRadii()[1];
        Rangefinder rangefinder = new Rangefinder(linkedUnit, rangeMax, rangeMin, linkedAbility.getLOSRequirement(), mapController, mapController.getTeamLists(), targetDirection);
        List<GameObject> hitUnits = rangefinder.generateTargetsNotOfTeam(linkedUnit.GetComponent<UnitController>().getTeam(), true);
        return hitUnits;
    }
}
