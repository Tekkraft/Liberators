using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCalculatorController : MonoBehaviour
{
    Grid mainGrid;
    MapController mapController;
    Canvas uiCanvas;
    Vector2Int calculatorPosition;

    GameObject linkedUnit;
    UnitController linkedController;
    Ability linkedAbility;

    public void inintalizeCalculator(GameObject unit, Ability ability, Vector2Int calcPos)
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        linkedUnit = unit;
        linkedController = linkedUnit.GetComponent<UnitController>();
        linkedAbility = ability;
        calculatorPosition = calcPos;
    }

    public List<GameObject> getAffectedUnits()
    {
        List<GameObject> hitUnits = new List<GameObject>();
        foreach (GameObject unit in mapController.getUnits())
        {
            int rangeMax = linkedAbility.getAbilityRadii()[0];
            int rangeMin = linkedAbility.getAbilityRadii()[1];
            Rangefinder rangefinder = new Rangefinder(linkedUnit, rangeMax, rangeMin, linkedAbility.getLOSRequirement(), mapController, mapController.getTeamLists());
            if (rangefinder.checkLineOfSightAOE(mapController.tileGridPos(calculatorPosition),unit))
            {
                hitUnits.Add(unit);
            }
        }
        return hitUnits;
    }

    void modifyEffect()
    {

    }
}
